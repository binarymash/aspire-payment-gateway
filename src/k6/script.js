import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

export let options = {
    stages: [
        { duration: '15s', target: 50 }, // ramp-up to 50 users
        { duration: '1m', target: 50 },  // stay at 50 users
        { duration: '15s', target: 0 },  // ramp-down to 0 users
    ],
    thresholds: {
        'errors': ['rate<0.1'], // <10% errors
        'http_req_duration': ['p(95)<5000'], // 95% of requests must complete below 500ms
    },
};

export function setup() {
    const authUrl = 'http://localhost:8080/realms/payment-gateway/protocol/openid-connect/token'; 
    const authPayload = 'grant_type=password&client_id=payment-gateway-customer&scope=email%20openid&username=test@test.com&password=123'

    const authParams = {
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
    };

    const authResponse = http.post(authUrl, authPayload, authParams);

    check(authResponse, {
        'is auth status 200': (r) => r.status === 200,
    });

    const token = authResponse.json('access_token');
    return { token };
}

export default function (data) {

    const url = 'https://localhost:7161/payments';
    const payload = JSON.stringify({
        card: {
            card_number: '4444333322221122',
            card_holder_name: 'Philip Wood',
            expiry: {
                month: 7,
                year: 2025
            },
            cvv: 123
        },
        payment: {
            amount: 999,
            currency_code: 'GBP'
        }
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${data.token}`,
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