export function getBoundingClientRects(htmlElements: HTMLElement[]): DOMRect[] {
    const result: DOMRect[] = [];

    for (const htmlElement of htmlElements)
        result.push(htmlElement.getBoundingClientRect());

    return result;
}
