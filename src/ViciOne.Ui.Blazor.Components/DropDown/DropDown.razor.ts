class DropDown {
    static get #maxVisibleItemCount() {
        return 10;
    }

    readonly #containerElement: HTMLElement;

    #inputElement: HTMLElement | undefined = undefined;
    #highlightedIndex = -1;
    #lastSelectedIndex = -1;

    constructor(containerElement: HTMLElement) {
        this.#containerElement = containerElement;
        this.#containerElement.addEventListener('click', this.#click);
    }

    public attachInputElement(inputElement: HTMLElement) {
        this.detachInputElement();

        this.#inputElement = inputElement;
        this.#inputElement.addEventListener('keydown', this.#keyDown);
    }

    public detachInputElement() {
        if (this.#inputElement) {
            this.#inputElement.removeEventListener('keydown', this.#keyDown);
            this.#inputElement = undefined;
        }
    }

    public reserveWidth() {
        this.#containerElement.style.minWidth = '';
        this.#containerElement.style.maxWidth = '';

        const { width } = this.#containerElement.getBoundingClientRect();

        this.#containerElement.style.minWidth = `${width}px`;
        this.#containerElement.style.maxWidth = `${width}px`;
    }

    public adjustMaxHeight(): boolean {
        const items = this.#getItemElements();

        if (items.length === 0)
            return false;

        const itemStyle = getComputedStyle(items[0]);

        const itemOuterHeight = items[0].getBoundingClientRect().height +
            parseFloat(itemStyle.marginTop) +
            parseFloat(itemStyle.marginBottom);

        if (itemOuterHeight <= 0)
            return false;

        const containerStyle = getComputedStyle(this.#containerElement);

        const containerVerticalExtra =
            parseFloat(containerStyle.borderTopWidth) +
            parseFloat(containerStyle.borderBottomWidth) +
            parseFloat(containerStyle.paddingTop) +
            parseFloat(containerStyle.paddingBottom);

        const visibleItemCount = Math.min(items.length, DropDown.#maxVisibleItemCount);

        this.#containerElement.style.maxHeight =
            `${(itemOuterHeight * visibleItemCount) + containerVerticalExtra}px`;

        return true;
    }

    public updatePlacement() {
        const inputElement = this.#inputElement;

        if (!inputElement)
            return;

        this.#containerElement.style.transform = '';

        const inputRect = inputElement.getBoundingClientRect();
        const containerRect = this.#containerElement.getBoundingClientRect();

        const viewportHeight = document.documentElement.clientHeight;

        const spaceBelow = viewportHeight - inputRect.bottom;
        const spaceAbove = inputRect.top;

        const overflowsBelow = containerRect.bottom > viewportHeight;
        const dropUp = overflowsBelow && spaceAbove > spaceBelow;

        if (dropUp) {
            const gap = Math.max(containerRect.top - inputRect.bottom, 0);
            const translateY = Math.trunc(inputRect.top - gap - containerRect.bottom);

            this.#containerElement.style.transform = `translateY(${translateY}px)`;
        }
    }

    public dispose() {
        this.detachInputElement();
        this.#containerElement.removeEventListener('click', this.#click);
    }

    #getItemElements(): HTMLElement[] {
        return Array.from(this.#containerElement.querySelectorAll<HTMLElement>('.drop-down-item'));
    }

    #isVisible(): boolean {
        return this.#containerElement.classList.contains('visible');
    }

    #getLastSelectedIndex(): number {
        if (this.#lastSelectedIndex >= 0)
            return this.#lastSelectedIndex;

        const items = this.#getItemElements();

        for (let index = items.length - 1; index >= 0; index--) {
            if (items[index].classList.contains('selected'))
                return index;
        }

        return -1;
    }

    #setHighlight(index: number) {
        const items = this.#getItemElements();

        if (this.#highlightedIndex >= 0 && this.#highlightedIndex < items.length)
            items[this.#highlightedIndex].classList.remove('highlighted');

        this.#highlightedIndex = index;

        if (this.#highlightedIndex >= 0 && this.#highlightedIndex < items.length) {
            items[this.#highlightedIndex].classList.add('highlighted');
            items[this.#highlightedIndex].scrollIntoView({ block: 'nearest' });
        }
    }

    readonly #click = (e: MouseEvent) => {
        if (!(e.target instanceof Element))
            return;

        const item = e.target.closest<HTMLElement>('.drop-down-item');

        if (!item)
            return;

        this.#lastSelectedIndex = item.classList.contains('selected') ?
            -1 :
            this.#getItemElements().indexOf(item);
    };

    readonly #keyDown = (e: KeyboardEvent) => {
        if (!this.#isVisible())
            return;

        const items = this.#getItemElements();
        const count = items.length;

        const isArrowDown = e.key === 'ArrowDown';

        if ((isArrowDown || e.key === 'ArrowUp') && count > 0) {
            let currentIndex = this.#highlightedIndex >= 0 ?
                this.#highlightedIndex :
                this.#getLastSelectedIndex();

            if (currentIndex >= count)
                currentIndex = -1;

            if (isArrowDown)
                this.#setHighlight((currentIndex + 1) % count);
            else
                this.#setHighlight(currentIndex <= 0 ? count - 1 : currentIndex - 1);

            e.preventDefault();

        } else if (e.key === 'Enter') {
            if (this.#highlightedIndex >= 0 && this.#highlightedIndex < count) {
                e.preventDefault();
                e.stopPropagation();

                items[this.#highlightedIndex].click();
                this.#setHighlight(-1);
            }

        } else {
            this.#setHighlight(-1);
        }
    };
}

export async function attach(containerElement: HTMLElement) {
    const dropDown = new DropDown(containerElement);

    return dropDown;
}
