
const form = document.getElementById('form');

form.addEventListener('submit', function(event) {
    event.preventDefault(); // Prevent default form submission

    const submit = document.getElementById('submit');
    submit.style.display = 'none';

    const responseArea = document.getElementById('responseArea');
    responseArea.innerHTML = 'Transforming...';

    const formData = new FormData(form);
    const jsonData = Object.fromEntries(formData.entries()); 

    console.log('json', jsonData);

    axios.post('/transform', jsonData, {
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        console.log('Success:', response.data);
        responseArea.innerHTML = response.data;

        submit.style.display = 'block';
    })
    .catch(error => {
        console.error('Error:', error);
    });
});
