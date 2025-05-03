import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
    stages: [
        { duration: '30s', target: 200 },
        { duration: '5m', target: 200 },
        { duration: '30s', target: 0 },
    ],
    thresholds: {
        http_req_duration: ['p(99)<100']
    }
};

export default () => {
    var start = Math.floor(Math.random() * 50) + 1;
    var end = Math.floor(Math.random() * 200) + 50;
    const url = 'http://localhost:5010/leaderboard?start=' + start + '&end=' + end;
    const res = http.get(url);
    check(res, { 'is status 200': (r) => r.status === 200 });
    sleep(1);
};