export class CaptureTargetRect {
    constructor(public left: number,
        public top: number,
        public right: number,
        public bottom: number) {}

    get width(): number {
        return this.right - this.left;
    }

    get height(): number {
        return this.bottom - this.top;
    }

    toDomRect(): DOMRect {
        return new DOMRect(this.left, this.top, this.width, this.height);
    }

    copy(): CaptureTargetRect {
        return new CaptureTargetRect(this.left, this.top, this.right, this.bottom);
    }
}
