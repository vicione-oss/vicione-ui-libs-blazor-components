/**
A value exchanged with the .NET side through Blazor that may be absent.

Blazor serializes a .NET `null` as `null`, never as `undefined`, so a strict check such as `value === undefined`
would miss it. Use this type only at that boundary; everywhere else the code base prefers `undefined`, which is why
writing `null` as a type is otherwise reported by ESLint.
*/
// eslint-disable-next-line @typescript-eslint/no-restricted-types
export type BlazorNullable<T> = T | null;
