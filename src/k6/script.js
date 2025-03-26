import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

export let options = {
    stages: [
        { duration: '15s', target: 10 }, // ramp-up to 10 users
        { duration: '1m', target: 10 },  // stay at 10 users
        { duration: '15s', target: 0 },  // ramp-down to 0 users
    ],
    thresholds: {
        'errors': ['rate<0.1'], // <10% errors
        'http_req_duration': ['p(95)<5000'], // 95% of requests must complete below 500ms
    },
};

export default function () {
    const url = 'https://localhost:7161/payments';
    const payload = JSON.stringify({
        Card: {
            CardNumber: '4444333322221122',
            CardHolderName: 'Philip Wood',
            Expiry: {
                Month: 7,
                Year: 2025
            },
            CVV: 123
        },
        Payment: {
            Amount: 999,
            CurrencyCode: 'GBP'
        }
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    let res = http.post(url, payload, params);

    const result = check(res, {
        'is status 200': (r) => r.status === 201,
    });

    errorRate.add(!result);

    sleep(1);
}

export function handleSummary(data) {
    const avg_latency = data.metrics.iteration_duration.values.avg;
    const min_latency = data.metrics.iteration_duration.values.min;
    const med_latency = data.metrics.iteration_duration.values.med;
    const max_latency = data.metrics.iteration_duration.values.max;
    const p90_latency = data.metrics.iteration_duration.values['p(90)'];
    const p95_latency = data.metrics.iteration_duration.values['p(95)'];

    const latency_message = `${avg_latency},${min_latency},${med_latency},${max_latency},${p90_latency},${p95_latency}\n`;

    return {
        stdout: latency_message,
    };
}