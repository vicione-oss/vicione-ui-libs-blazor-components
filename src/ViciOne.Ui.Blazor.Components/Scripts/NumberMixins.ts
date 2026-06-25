// https://www.typescriptlang.org/docs/handbook/mixins.html#alternative-pattern

class NumberMixins {
    clamp(this: number, minimum: number, maximum: number): number {
        return Math.min(Math.max(this.valueOf(), minimum), maximum);
    }
}

// eslint-disable-next-line @typescript-eslint/consistent-type-definitions, no-unused-vars
interface Number extends NumberMixins { }

// eslint-disable-next-line no-extend-native
Object.defineProperties(Number.prototype, Object.getOwnPropertyDescriptors(NumberMixins.prototype));
