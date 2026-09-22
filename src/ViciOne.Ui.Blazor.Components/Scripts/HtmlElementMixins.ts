// https://www.typescriptlang.org/docs/handbook/mixins.html#alternative-pattern

class HtmlElementMixins {
    /**
     Calculates the bounding client rectangle of one of the element's pseudo-elements
     (::before or ::after), expressed in viewport coordinates like getBoundingClientRect().

     A fixed pseudo-element is positioned against the viewport, otherwise against the host box.
     When a dimension is not expressed in pixels it falls back to the host's size (or the
     viewport size for fixed pseudo-elements), matching full-cover backdrops.

     @param pseudoElement - Which pseudo-element to measure, '::before' or '::after'.
     @returns The pseudo-element's bounding rectangle, or undefined when it is not rendered
     (for example 'content: none').
     */
    getPseudoElementBoundingClientRect(this: HTMLElement, pseudoElement: '::before' | '::after'): DOMRect | undefined {
        const style = getComputedStyle(this, pseudoElement);

        // 'content: none' means the pseudo-element is not rendered, so it has no box.
        if (style.content === 'none')
            return undefined;

        const hostBounds = this.getBoundingClientRect();

        // Fixed elements are measured from the viewport corner; others from the host's corner.
        const isFixed = style.position === 'fixed';
        const originLeft = isFixed ? 0 : hostBounds.left;
        const originTop = isFixed ? 0 : hostBounds.top;
        const fallbackWidth = isFixed ? window.innerWidth : hostBounds.width;
        const fallbackHeight = isFixed ? window.innerHeight : hostBounds.height;

        // ToPixels() is a String mixin.
        //
        // No import of 'StringMixins.ts' on purpose: it would make this file
        // a module and break the global `interface HTMLElement` merge.
        //
        // Consumers need to import 'StringMixins.ts' to load the prototype patch.
        const left = originLeft + (style.left.toPixels() ?? 0);
        const top = originTop + (style.top.toPixels() ?? 0);
        const width = style.width.toPixels() ?? fallbackWidth;
        const height = style.height.toPixels() ?? fallbackHeight;

        return new DOMRect(left, top, width, height);
    }
}

// eslint-disable-next-line @typescript-eslint/consistent-type-definitions, @typescript-eslint/naming-convention
interface HTMLElement extends HtmlElementMixins { }

Object.defineProperties(HTMLElement.prototype, Object.getOwnPropertyDescriptors(HtmlElementMixins.prototype));
