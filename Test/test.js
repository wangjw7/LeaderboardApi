import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
    vus: 5,
    duration: '2s'
};

export default () => {
    var score = Math.floor(Math.random() * 20) - 5;
    var id = Math.floor(Math.random() * 100000);
    const url = 'http://localhost:5010/customer/' + id + '/score/' + score;
    var res = http.post(url);
    check(res, { 'is status 200': (r) => r.status === 200 });
    console.log(res.status_text);
    console.log(res.body);
    //sleep(1);
};