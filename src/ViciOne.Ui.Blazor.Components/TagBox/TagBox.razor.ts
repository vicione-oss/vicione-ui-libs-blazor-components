class TagBox {
    readonly #tagBoxElement: HTMLDivElement;
    readonly #tagInputElement: HTMLInputElement;

    #highlightedIndex = -1;

    constructor(tagBoxElement: HTMLDivElement, tagInputElement: HTMLInputElement) {
        this.#tagBoxElement = tagBoxElement;
        this.#tagInputElement = tagInputElement;

        this.#tagBoxElement.addEventListener('click', this.#tagBoxClick);
        this.#tagInputElement.addEventListener('keydown', this.#inputKeyDown);
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

    public getHighlightedIndex(): number {
        return this.#highlightedIndex;
    }

    public resetHighlight() {
        this.#setHighlight(-1);
    }

    public dispose() {
        this.#tagBoxElement.removeEventListener('click', this.#tagBoxClick);
        this.#tagInputElement.removeEventListener('keydown', this.#inputKeyDown);
        this.#tagInputElement.removeEventListener('input', this.#inputChanged);
    }

    #getAvailableTagElements(): HTMLElement[] {
        const container = this.#tagBoxElement.querySelector('.available-tags-dropdown-container');

        const availableTagElements = container ? Array.from(container.querySelectorAll<HTMLElement>('.available-tag')) : [];

        return availableTagElements;
    }

    #setHighlight(index: number) {
        const items = this.#getAvailableTagElements();

        if (this.#highlightedIndex >= 0 && this.#highlightedIndex < items.length)
            items[this.#highlightedIndex].classList.remove('highlighted');

        this.#highlightedIndex = index;

        if (this.#highlightedIndex >= 0 && this.#highlightedIndex < items.length) {
            items[this.#highlightedIndex].classList.add('highlighted');
            items[this.#highlightedIndex].scrollIntoView({ block: 'nearest' });
        }
    }

    readonly #tagBoxClick = (_: MouseEvent) => {
        this.#tagInputElement.focus();
    };

    readonly #inputKeyDown = (e: KeyboardEvent) => {
        const items = this.#getAvailableTagElements();
        const count = items.length;

        const isArrowDown = e.key === 'ArrowDown';

        if ((isArrowDown || e.key === 'ArrowUp') && count > 0) {
            if (isArrowDown)
                this.#setHighlight((this.#highlightedIndex + 1) % count);
            else
                this.#setHighlight(this.#highlightedIndex <= 0 ? count - 1 : this.#highlightedIndex - 1);

            e.preventDefault();

        } else if (e.key !== 'Enter') {
            this.#setHighlight(-1);
        }
    };

    readonly #inputChanged = () => {
        this.alignInputElement();
    };
}

export async function attach(tagBoxElement: HTMLDivElement, tagInputElement: HTMLInputElement) {
    const tagBox = new TagBox(tagBoxElement, tagInputElement);

    return tagBox;
}

