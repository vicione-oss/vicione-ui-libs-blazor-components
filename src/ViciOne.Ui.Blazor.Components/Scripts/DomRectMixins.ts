// https://www.typescriptlang.org/docs/handbook/mixins.html#alternative-pattern

class DomRectMixins {
    /**
     * Calculates the overlapping region of this rectangle and another.
     *
     * @param other - The rectangle to intersect with.
     * @returns The overlapping rectangle, or undefined when the two rectangles do not overlap.
     */
    intersect(this: DOMRect, other: DOMRect): DOMRect | undefined {
        const left = Math.max(this.left, other.left);
        const top = Math.max(this.top, other.top);
        const right = Math.min(this.right, other.right);
        const bottom = Math.min(this.bottom, other.bottom);

        if (right <= left || bottom <= top)
            return undefined;

        return new DOMRect(left, top, right - left, bottom - top);
    }
}

// eslint-disable-next-line @typescript-eslint/consistent-type-definitions, @typescript-eslint/naming-convention
interface DOMRect extends DomRectMixins { }

Object.defineProperties(DOMRect.prototype, Object.getOwnPropertyDescriptors(DomRectMixins.prototype));
