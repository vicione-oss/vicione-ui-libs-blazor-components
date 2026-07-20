import { ComputedBackgroundColor } from '/_content/ViciOne.Ui.Blazor.Components/js/computed-background-color.js';
import '/_content/ViciOne.Ui.Blazor.Components/js/html-element-mixins.js';

const computedBackgroundColor = new ComputedBackgroundColor();

export function applyComputedBackgroundColor(element: HTMLElement): void {
    // ComputedBackgroundColor probes the page with document.elementsFromPoint, which only returns
    // results while the probe point is inside the viewport. If we resolved as soon as the element
    // started intersecting (default threshold 0), most of its probe points would still be below the
    // fold and fall back to the page background, skewing the result. Waiting for the element to be
    // fully visible (threshold 1.0) ensures every probe point is on-screen before resolving.
    const observer = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (entry.isIntersecting && entry.intersectionRatio >= 1)
                resolve(element);
        }
    }, { threshold: 1 });

    observer.observe(element);
}

function resolve(element: HTMLElement): void {
    const backgroundColor = computedBackgroundColor.resolve(
        element,
        element.getPseudoElementBoundingClientRect('::before')
    );

    if (!backgroundColor)
        return;

    // Set the resolved color on the surrounding demo section so the overflow gradient picks it up,
    // falling back to the element itself when no section wraps it.
    const target = element.closest<HTMLElement>('.demo') ?? element;
    target.style.setProperty('--overflow-background', backgroundColor);
}
