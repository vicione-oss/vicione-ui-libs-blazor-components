export const getBoundingClientRects = (htmlElements: HTMLElement[]): DOMRect[] =>
    htmlElements.map(htmlElement => htmlElement.getBoundingClientRect());
