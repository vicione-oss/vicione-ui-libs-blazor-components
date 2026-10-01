import { type TableColumn } from './TableColumn.ts';

// The minimum is read once up front, so the redistribution passes never go back to the DOM.
export type ResolvedColumnWidth = {
    readonly flexColumn: TableColumn;
    readonly minimumWidth: number;
    width: number;
};
