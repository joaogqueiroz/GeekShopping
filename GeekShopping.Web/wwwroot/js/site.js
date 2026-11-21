// Quantity stepper on the product page: the − and + buttons change the number field
document.querySelectorAll('[data-stepper]').forEach(stepper => {
    const input = stepper.querySelector('input');
    stepper.querySelectorAll('[data-step]').forEach(button => {
        button.addEventListener('click', () => {
            const min = Number(input.min) || 1;
            const max = Number(input.max) || 100;
            const next = (Number(input.value) || min) + Number(button.dataset.step);
            input.value = Math.min(max, Math.max(min, next));
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });
    });
});

// Product form: keeps the store card preview in step with the fields
const card = document.querySelector('[data-preview-card]');
if (card) {
    const show = name => card.querySelector(`[data-show="${name}"]`);
    const money = new Intl.NumberFormat(document.documentElement.lang || 'en', { style: 'currency', currency: 'BRL' });
    document.querySelectorAll('[data-preview]').forEach(field => {
        field.addEventListener('input', () => {
            const kind = field.dataset.preview;
            const value = field.value.trim();
            if (kind === 'img') {
                show('img').src = value || '/images/product-placeholder.png';
            } else if (kind === 'price') {
                const number = Number(value.replace(',', '.'));
                show('price').textContent = Number.isFinite(number) ? money.format(number) : value;
            } else {
                show(kind).textContent = value;
            }
        });
    });
}
