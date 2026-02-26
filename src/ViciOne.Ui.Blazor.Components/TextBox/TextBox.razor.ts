class TextBox {
    /**
     * Selects the content of the given input
     */
    selectContent(input: HTMLInputElement) {
        input.select();
    }
}

export function attach() {
    return new TextBox();
}
