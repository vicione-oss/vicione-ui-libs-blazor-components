export class ArrayIterator<T> {
    private i = 0;

    constructor(readonly array: T[]) {
    }

    public next(): IteratorResult<T> {
        if (this.i < this.array.length)
            return { value: this.array[this.i++], done: false };

        return { value: null, done: true };
    }
}
