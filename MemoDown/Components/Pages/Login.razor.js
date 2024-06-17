export function onLoad() {
    console.log('Loaded');
}

export function onUpdate() {
    console.log('Updated');
    turnstile?.reset()
}

export function onDispose() {
    console.log('Disposed');
}