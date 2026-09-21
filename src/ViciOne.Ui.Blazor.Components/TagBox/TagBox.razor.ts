export class TagBox {
    readonly #tagBoxElement: HTMLDivElement;
    readonly #tagInputElement: HTMLInputElement;

    readonly #tagBoxClick = (_: MouseEvent) => {
        this.#tagInputElement.focus();
    };

    readonly #inputChanged = () => {
        this.alignInputElement();
    };

    constructor(tagBoxElement: HTMLDivElement, tagInputElement: HTMLInputElement) {
        this.#tagBoxElement = tagBoxElement;
        this.#tagInputElement = tagInputElement;

        this.#tagBoxElement.addEventListener('click', this.#tagBoxClick);
        this.#tagInputElement.addEventListener('input', this.#inputChanged);
    }

    public alignInputElement() {
        const offset = 10;

        // Collapse the input before measuring so scrollWidth reflects the actual typed
        // text width and not the width the flex-grown input currently fills. Without
        // collapsing, scrollWidth equals the already reserved/filled width, so adding
        // `offset` would widen the whole control by `offset` on the very first input.
        this.#tagInputElement.style.removeProperty('flex-basis');
        this.#tagInputElement.style.flex = '0 0 0';
        this.#tagInputElement.style.width = '0';

        const { scrollWidth } = this.#tagInputElement;

        this.#tagInputElement.style.removeProperty('flex');

        let shouldPerformLineBreak = false;

        // Set current width of input box with overflow + offset to suppress bouncing effect
        this.#tagInputElement.style.width = scrollWidth + offset + 'px';

        // Check if input is too wide for the actual input width
        // Use Math.ceil to round up to the next higher integer, since scrollWidth returns integers,
        // to avoid rounding issues and prevent unnecessary line breaks
        // e.g. line with 4 tags -> input box width = 33.33px -> scrollWidth returns 34
        if (scrollWidth > Math.ceil(Number.parseFloat(getComputedStyle(this.#tagInputElement).width)))
            shouldPerformLineBreak = true;

        if (shouldPerformLineBreak) {
            // Perform line break if input is too long
            this.#tagInputElement.style.flexBasis = '100%';
        }
    }

    public dispose() {
        this.#tagBoxElement.removeEventListener('click', this.#tagBoxClick);
        this.#tagInputElement.removeEventListener('input', this.#inputChanged);
    }
}
