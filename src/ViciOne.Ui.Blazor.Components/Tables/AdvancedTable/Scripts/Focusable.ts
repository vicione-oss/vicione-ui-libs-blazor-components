// What takes focus inside a cell or a filter panel. A `tabindex="-1"` is honored as the "do not focus when
// tabbing" it means everywhere else in this library — `SpinEdit`'s spin buttons and `SearchBox`'s clear
// button carry it — so the walk inside an entered cell steps over those and a cell with one real control
// returns to itself on the first press.
export const focusableSelector = ':is(input:not([type="hidden"]), select, textarea, button, a[href])' +
    ':not(:disabled):not([tabindex="-1"]), [tabindex="0"]';

export function focusFirstFocusable(container: HTMLElement): void {
    // First match in DOM order, not in selector order — a two-input range editor focuses its lower bound, and
    // an editor opening on a select focuses that. Disabled and hidden elements are excluded because .focus() on
    // either is a silent no-op.
    container.querySelector<HTMLElement>(focusableSelector)?.focus();
}
