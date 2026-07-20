// https://www.typescriptlang.org/docs/handbook/mixins.html#alternative-pattern

import type { RgbaColor } from '/_content/ViciOne.Ui.Blazor.Components/js/rgba-color.js';

class StringMixins {
    // True when this string is a CSS pixel value (e.g. "12px").
    isPixelValue(this: string): boolean {
        return this.valueOf().endsWith('px');
    }

    // Reads a CSS pixel value (e.g. "12px") as a number, or undefined when it is not in pixels.
    toPixels(this: string): number | undefined {
        if (!this.isPixelValue())
            return undefined;

        const parsed = Number.parseFloat(this.valueOf());
        return Number.isFinite(parsed) ? parsed : undefined;
    }

    // Reads a CSS color string like "rgb(10, 20, 30)" or "rgba(10, 20, 30, 0.5)" into an RgbaColor.
    // Returns undefined when the string has no usable numbers. A missing alpha defaults to 1 (opaque).
    toRgbaColor(this: string): RgbaColor | undefined {
        const numbers = this.valueOf().match(/[\d.]+/g);
        if (!numbers || numbers.length < 3)
            return undefined;

        const alpha = numbers.length >= 4 ? Number(numbers[3]) : 1;
        return {
            red: Number(numbers[0]),
            green: Number(numbers[1]),
            blue: Number(numbers[2]),
            alpha
        };
    }
}

// Top-level imports in this file make it become a module, which breaks
// the global `interface HTMLElement` merge required for use of mixins.
//
// We add a little escape hatch here that re-opens the global scope,
// so the augmentation lands on the real global String again.
declare global {
    // eslint-disable-next-line @typescript-eslint/consistent-type-definitions, no-unused-vars
    interface String extends StringMixins { }
}

// eslint-disable-next-line no-extend-native
Object.defineProperties(String.prototype, Object.getOwnPropertyDescriptors(StringMixins.prototype));
