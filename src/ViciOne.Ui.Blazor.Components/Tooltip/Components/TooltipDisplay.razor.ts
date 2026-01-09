/**
 * Gets the size of the tooltip with the provided id
 * @param {string} tooltipId
*/
export function getTooltipSize(tooltipId: string) {
    const elem = document.getElementById(tooltipId);

    if (!elem)
        return { height: 0, width: 0 };

    return {
        height: elem.offsetHeight,
        width: elem.offsetWidth
    };
}

/**
 * Gets the size of the current window
 */
export function getWindowSize() {
    return {
        height: window.innerHeight,
        width: window.innerWidth
    };
}
