export type CloseObserver = {
    readonly cleanup: () => void;
    readonly requestClose: () => void;
};
