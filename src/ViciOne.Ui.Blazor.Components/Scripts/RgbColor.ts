export class RgbColor {
    constructor(
        public readonly red: number,
        public readonly green: number,
        public readonly blue: number
    ) {}

    // Returns a CSS "rgb(r, g, b)" color string, clamping each channel to 0-255.
    toCssValue(): string {
        const toChannel = (value: number) => Math.max(0, Math.min(255, Math.round(value)));
        return `rgb(${toChannel(this.red)}, ${toChannel(this.green)}, ${toChannel(this.blue)})`;
    }
}
