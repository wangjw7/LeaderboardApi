import http from 'k6/http'

export const options = {
    vus: 5,
    duration: '2s'
};

export default () => {
    var score = Math.floor(Math.random() * 1000);
    var id = Math.floor(Math.random() * 100000);
    const url = 'http://localhost:5010/customer/' + id + '/score/' + score;
    var res = http.post(url);
    console.log(res.status_text);
    console.log(res.body);
};