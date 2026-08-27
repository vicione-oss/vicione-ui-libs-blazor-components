/**
 Gets the size of the tooltip with the provided id
 @param tooltipId The Identifier of the tooltip
*/
export function getTooltipSize(tooltipId: string) {
    const element = document.querySelector<HTMLElement>(`#${CSS.escape(tooltipId)}`);

    return {
        height: element?.offsetHeight ?? 0,
        width: element?.offsetWidth ?? 0
    };
}

/**
 Gets the size of the current window
 */
export function getWindowSize() {
    return {
        height: window.innerHeight,
        width: window.innerWidth
    };
}
