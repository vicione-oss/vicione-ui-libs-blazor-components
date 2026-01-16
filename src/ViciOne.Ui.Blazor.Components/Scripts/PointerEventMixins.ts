// https://www.typescriptlang.org/docs/handbook/mixins.html#alternative-pattern

class PointerEventMixins {
    /**
     * @param rootAncestor - Element that potentially contains the element having raised the pointer event
     * @param featureModifierClass - CSS class that identifies the required feature on the nested element like 'moveable'
     */
    isRaisedByNestableOf(this: PointerEvent, rootAncestor: HTMLElement, featureModifierClass: string): boolean {
        // If event was raised by a nested element ...
        if (this.currentTarget !== this.target && this.target instanceof HTMLElement) {

            // ... and if this nested element has the required feature ...
            if (this.target.classList.contains(featureModifierClass))
                return true; // ... then we just do nothing as this nested element has its own handler for the feature

            // ... otherwise we traverse through the ancestors ...
            let ancestor = this.target.parentElement;
            while (ancestor
                && ancestor !== rootAncestor // ... as long as the we don't find our root ancestor ...
                && !ancestor.classList.contains(featureModifierClass)) { // ... and the ancestor does not have the required feature

                ancestor = ancestor.parentElement;
            }

            // If the resulting ancestor is not null and not the given root ancestor ...
            if (ancestor && ancestor !== rootAncestor)
                return true; // ... then we just do nothing as the ancestor is a nested ancestor with having its own handler for the required feature
        }

        return false;
    }

    isModifierKeyPressed(this: PointerEvent): boolean {
        return this.altKey || this.ctrlKey || this.shiftKey;
    }
}

// eslint-disable-next-line @typescript-eslint/consistent-type-definitions
interface PointerEvent extends PointerEventMixins { }

applyMixins(PointerEvent, [PointerEventMixins]);

function applyMixins(derivedCtor: any, constructors: any[]) {
    constructors.forEach(baseCtor => {
        Object.getOwnPropertyNames(baseCtor.prototype).forEach(name => {
            Object.defineProperty(
                derivedCtor.prototype,
                name,
                // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
                Object.getOwnPropertyDescriptor(baseCtor.prototype, name) ?? Object.create(null)
            );
        });
    });
}
