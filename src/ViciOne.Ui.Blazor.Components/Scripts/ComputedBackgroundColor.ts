import './StringMixins.ts';
import './NumberMixins.ts';
import './DomRectMixins.ts';
import { RgbColor } from './RgbColor.ts';
import type { RgbaColor } from './RgbaColor.ts';

/**
 Finds the real background color behind an element, even when several see-through layers overlap.
 Stateless: call refreshBackgroundColor() whenever the value needs to be recomputed
 (for example when the observed element changes size).
 */
export class ComputedBackgroundColor {
    // Collects every background layer stacked at a point (top to bottom, stopping at the first opaque
    // one) and blends them into a single solid color, falling back to the page background.
    #resolveColorAtPoint(
        startElement: HTMLElement, pointX: number, pointY: number
    ): RgbColor | undefined {
        const layerStyles = this.#collectLayerStyles(startElement, pointX, pointY);

        // Turn each source into an RGBA color, keeping only the visible ones.
        const layers: RgbaColor[] = [];
        for (const style of layerStyles) {
            const color = this.#resolveLayerColor(style);
            if (color)
                layers.push(color);

            // A fully opaque layer hides everything behind it, so we can stop here.
            if (color?.alpha === 1)
                break;
        }

        if (layers.length === 0)
            return this.#resolvePageBackgroundColor();

        const bottomLayer = layers.at(-1);

        if (!bottomLayer)
            return this.#resolvePageBackgroundColor();

        const isBottomLayerOpaque = bottomLayer.alpha === 1;

        // Start from the bottom opaque layer, or the page background when the whole stack is
        // see-through (the page background is painted on the viewport, not a real element, so it never
        // shows up in hit-testing; without it dark themes would wash out over an assumed white page).
        let color: RgbColor = isBottomLayerOpaque ?
            new RgbColor(bottomLayer.red, bottomLayer.green, bottomLayer.blue) :
            this.#resolvePageBackgroundColor();

        // Blend each remaining layer from bottom to top over the accumulated color.
        const topLayerIndex = isBottomLayerOpaque ?
            layers.length - 2 :
            layers.length - 1;

        for (let index = topLayerIndex; index >= 0; index--) {
            const layer = layers[index];
            color = new RgbColor(
                (layer.red * layer.alpha) + (color.red * (1 - layer.alpha)),
                (layer.green * layer.alpha) + (color.green * (1 - layer.alpha)),
                (layer.blue * layer.alpha) + (color.blue * (1 - layer.alpha))
            );
        }

        return color;
    }

    // Builds the background styles behind the element at a point, topmost first. Each element at the
    // point contributes its own background plus its ::before/::after pseudo-elements
    #collectLayerStyles(startElement: HTMLElement, pointX: number, pointY: number): CSSStyleDeclaration[] {
        const elementsAtPoint = document.elementsFromPoint(pointX, pointY);

        // If our element is not painted here (e.g. the point lands in a rounded-corner cut-out), it has
        // no background; an empty list avoids wrongly averaging in the page background.
        const startIndex = elementsAtPoint.indexOf(startElement);
        if (startIndex === -1)
            return [];

        // Keep only our element and everything behind it; skip whatever is painted on top.
        const elements = elementsAtPoint.slice(startIndex);

        const styles: CSSStyleDeclaration[] = [];

        for (const element of elements) {
            if (!(element instanceof HTMLElement))
                continue;

            styles.push(...this.#resolveElementStyles(element, pointX, pointY));
        }

        return styles;
    }

    // Resolves one element's own background plus its ::before/::after pseudo-elements, topmost first.
    // The own background is at the bottom; pseudo-elements paint above it, ordered by z-index (::after
    // above ::before on a tie). Pseudo-elements that don't render or cover the point are skipped.
    #resolveElementStyles(element: HTMLElement, pointX: number, pointY: number): CSSStyleDeclaration[] {
        // Look at ::before then ::after; keep only the ones that actually render and cover the point.
        const pseudoStyles: CSSStyleDeclaration[] = [];
        for (const pseudoElement of ['::before', '::after']) {
            const style = getComputedStyle(element, pseudoElement);

            // 'content: none' means the pseudo-element is not rendered, so it paints nothing.
            if (style.content === 'none')
                continue;

            // Skip overlays that don't cover the point (for example a small badge somewhere else).
            if (!this.#pseudoElementCoversPoint(element, style, pointX, pointY))
                continue;

            pseudoStyles.push(style);
        }

        // Ascending z-index (stable sort keeps ::before below ::after), matching CSS paint order.
        pseudoStyles.sort((first, second) => this.#parseZindex(first) - this.#parseZindex(second));

        // Own background first, then pseudo-elements above it; reverse to topmost-first.
        const bottomToTop: CSSStyleDeclaration[] = [getComputedStyle(element), ...pseudoStyles];

        // Switch to `bottomToTop.toReversed()` once the TS compiler/lib target supports it reliably.
        // eslint-disable-next-line unicorn/no-array-reverse
        return bottomToTop.reverse();
    }

    // Reads a computed z-index as a number. 'auto' (and anything non-numeric) becomes 0 so elements
    // without an explicit z-index are compared purely by their order.
    #parseZindex(style: CSSStyleDeclaration): number {
        const zIndex = Number.parseInt(style.zIndex, 10);
        return Number.isNaN(zIndex) ? 0 : zIndex;
    }

    // Reads the page's background color from <body> (or <html>), used as the opaque base when no
    // opaque layer is found. Defaults to white when neither has an opaque background.
    #resolvePageBackgroundColor(): RgbColor {
        for (const element of [document.body, document.documentElement]) {
            const color = getComputedStyle(element).backgroundColor.toRgbaColor();
            if (color?.alpha === 1)
                return new RgbColor(color.red, color.green, color.blue);
        }

        return new RgbColor(255, 255, 255);
    }

    // Turns a background style into its RGBA color, or undefined when it paints nothing. The color's
    // alpha is multiplied by the element's own opacity so see-through elements blend correctly.
    #resolveLayerColor(style: CSSStyleDeclaration): RgbaColor | undefined {
        const backgroundColor = style.backgroundColor.toRgbaColor();
        if (!backgroundColor)
            return undefined;

        const opacity = Number(style.opacity);
        const effectiveAlpha = backgroundColor.alpha * (Number.isFinite(opacity) ? opacity : 1);
        if (effectiveAlpha <= 0)
            return undefined;

        return {
            red: backgroundColor.red, green: backgroundColor.green, blue: backgroundColor.blue, alpha: effectiveAlpha
        };
    }

    // Checks whether a pseudo-element's box contains the probe point. Fixed pseudo-elements are
    // positioned against the viewport, others against the host box. A size that isn't in pixels is
    // assumed to cover the whole box (matching full-cover backdrops).
    #pseudoElementCoversPoint(
        host: HTMLElement, style: CSSStyleDeclaration, pointX: number, pointY: number
    ): boolean {
        const hostBounds = host.getBoundingClientRect();

        // Fixed elements are measured from the viewport corner; others from the host's corner.
        const isFixed = style.position === 'fixed';
        const originLeft = isFixed ? 0 : hostBounds.left;
        const originTop = isFixed ? 0 : hostBounds.top;
        const fallbackWidth = isFixed ? window.innerWidth : hostBounds.width;
        const fallbackHeight = isFixed ? window.innerHeight : hostBounds.height;

        const left = originLeft + (style.left.toPixels() ?? 0);
        const top = originTop + (style.top.toPixels() ?? 0);

        const width = style.width.toPixels() ?? fallbackWidth;
        const height = style.height.toPixels() ?? fallbackHeight;

        return pointX >= left && pointX <= left + width &&
            pointY >= top && pointY <= top + height;
    }

    /**
    Resolves the background color behind the element (or the given probe region) Call this
    whenever the element or its probe region may have changed (for example on resize).

    @param element - The element whose background is written to.
    @param probeBounds - Optional region to probe. When omitted, the element's own bounding
    rectangle is used. Supply this to probe a sub-region (for example a gradient
    pseudo-element) that needs its background resolved independently of the host box.
    */
    public resolve(
        element: HTMLElement | undefined, probeBounds?: DOMRect
    ): string | undefined {
        if (element === undefined)
            return undefined;

        // Probe the caller-provided region when supplied (for example a gradient pseudo-element),
        // otherwise fall back to the element's own box.
        const bounds = probeBounds ?? element.getBoundingClientRect();
        if (bounds.width === 0 || bounds.height === 0)
            return undefined;

        // Document.elementsFromPoint only returns results for points inside the viewport, so clip the
        // probe region to the viewport first. Probing the clipped rectangle keeps every probe point
        // on-screen (partially visible regions resolve from their visible part), and a region entirely
        // off-screen produces an empty intersection and no color.
        const viewportWidth = window.innerWidth === 0 ? document.documentElement.clientWidth : window.innerWidth;
        const viewportHeight = window.innerHeight === 0 ? document.documentElement.clientHeight : window.innerHeight;
        const visibleBounds = bounds.intersect(new DOMRect(0, 0, viewportWidth, viewportHeight));
        if (!visibleBounds)
            return undefined;

        // Inset the corners one pixel on every edge, so probing stays off the box boundary (which is
        // shared with adjacent elements) and off the exclusive right/bottom edges that hit-testing
        // can miss under fractional zoom coordinates.
        const left = visibleBounds.left + 1;
        const top = visibleBounds.top + 1;
        const right = visibleBounds.right - 1;
        const bottom = visibleBounds.bottom - 1;

        const probePoints = [
            { x: left, y: top },
            { x: right, y: top },
            { x: left, y: bottom },
            { x: right, y: bottom },
            { x: (visibleBounds.left + visibleBounds.right) / 2, y: (visibleBounds.top + visibleBounds.bottom) / 2 }
        ];

        const colors: RgbColor[] = [];
        for (const point of probePoints) {
            const color = this.#resolveColorAtPoint(element, point.x, point.y);
            if (color)
                colors.push(color);
        }

        if (colors.length === 0)
            return undefined;

        const averageColor = new RgbColor(
            colors.map(color => color.red).average(),
            colors.map(color => color.green).average(),
            colors.map(color => color.blue).average()
        );

        return averageColor.toCssValue();
    }
}
