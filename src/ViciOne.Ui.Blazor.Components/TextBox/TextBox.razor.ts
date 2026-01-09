class TextBox {
    /**
     * Selects the content of the given input
     */
    public selectContent(input: HTMLInputElement) {
        input.select();
    }
}

export async function attach() {
    const textBox = new TextBox();

    return textBox;
}
