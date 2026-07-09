class TagBox {
    readonly #tagBoxElement: HTMLDivElement;
    readonly #tagInputElement: HTMLInputElement;

    constructor(tagBoxElement: HTMLDivElement, tagInputElement: HTMLInputElement) {
        this.#tagBoxElement = tagBoxElement;
        this.#tagInputElement = tagInputElement;

        this.#tagBoxElement.addEventListener('click', this.#tagBoxClick);
        this.#tagInputElement.addEventListener('input', this.#inputChanged);
    }

    public alignInputElement() {
        this.#tagInputElement.style.removeProperty('flex-basis');
        this.#tagInputElement.style.removeProperty('width');

        const offset = 10;
        const { scrollWidth } = this.#tagInputElement;

        let lineBreak = false;

        // Set current width of input box with overflow + offset to suppress bouncing effect
        this.#tagInputElement.style.width = scrollWidth + offset + 'px';

        // Check if input is too wide for the actual input width
        // Use Math.ceil to round up to the next higher integer, since scrollWidth returns integers,
        // to avoid rounding issues and prevent unnecessary line breaks
        // e.g. line with 4 tags -> input box width = 33.33px -> scrollWidth returns 34
        if (scrollWidth > Math.ceil(parseFloat(getComputedStyle(this.#tagInputElement).width)))
            lineBreak = true;

        if (lineBreak) {
            // Perform line break if input is too long
            this.#tagInputElement.style.flexBasis = '100%';
        }
    }

    public dispose() {
        this.#tagBoxElement.removeEventListener('click', this.#tagBoxClick);
        this.#tagInputElement.removeEventListener('input', this.#inputChanged);
    }

    readonly #tagBoxClick = (_: MouseEvent) => {
        this.#tagInputElement.focus();
    };

    readonly #inputChanged = () => {
        this.alignInputElement();
    };
}

export async function attach(tagBoxElement: HTMLDivElement, tagInputElement: HTMLInputElement) {
    const tagBox = new TagBox(tagBoxElement, tagInputElement);

    return tagBox;
}
