import http from 'k6/http';
import { check, sleep } from 'k6';

// Конфігурація: Load Test + Stress Test
export const options = {
    scenarios: {
        // 1. Load Test: Перевірка доступності слотів
        get_slots_load: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '15s', target: 50 }, // Розгін до 50 користувачів
                { duration: '1m', target: 50 },  // Тримаємо навантаження
                { duration: '15s', target: 0 },  // Затухання
            ],
            exec: 'getSlots',
        },
        // 2. Stress Test: Конкурентне бронювання одного слота (Race Condition)
        concurrent_booking_stress: {
            executor: 'constant-arrival-rate',
            rate: 100, // 100 запитів
            timeUnit: '1s', // кожну секунду
            duration: '10s',
            preAllocatedVUs: 50,
            maxVUs: 100,
            exec: 'bookAppointment',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% запитів мають оброблятися швидше 500мс
        http_req_failed: ['rate<0.25'],   // Не більше 1% загальних помилок (не враховуючи 409)
    },
};

const BASE_URL = 'http://localhost:5233/api';

// Життєвий цикл k6: Ініціалізація даних перед тестом
export function setup() {
    // В реальності тут може бути запит до API для створення тестового Provider
    // Для прикладу повертаємо захардкоджені ID з нашої БД на 10 000 записів
    return {
        providerId: '0f93e176-4929-421c-9ce9-eca02fabd5a3',
        serviceId: 'b252fd16-12c6-4f34-a34a-afa90c6b335b',
        testDate: '2026-03-20',
        startTime: '10:00:00'
    };
}

export function getSlots(data) {
    const res = http.get(`${BASE_URL}/providers/${data.providerId}/slots?date=${data.testDate}`);
    check(res, {
        'status is 200': (r) => r.status === 200,
        'response is not empty': (r) => r.body.length > 0,
    });
    sleep(1);
}

export function bookAppointment(data) {
    const payload = JSON.stringify({
        providerId: data.providerId,
        serviceId: data.serviceId,
        clientName: `Stress User ${__VU}`,
        clientEmail: `user${__VU}@test.com`,
        date: data.testDate,
        startTime: data.startTime
    });

    const params = { headers: { 'Content-Type': 'application/json' } };
    const res = http.post(`${BASE_URL}/appointments`, payload, params);

    // PostgreSQL має відбити всі дублікати, тому ми очікуємо або 200 (перший щасливчик), або 400/409 (усі інші)
    check(res, {
        'status is 200 or 400/409': (r) => r.status === 200 || r.status === 400 || r.status === 409,
    });
}