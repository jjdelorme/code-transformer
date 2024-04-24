const responseArea = document.getElementById('responseArea');
const responseLabel = document.getElementById('responseLabel');
const form = document.getElementById('form');
const submit = document.getElementById('submit');

form.addEventListener('submit', function(event) {
    event.preventDefault(); // Prevent default form submission

    responseLabel.innerHTML = 'Transforming...';
    submit.style.display = 'none';

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

        responseArea.mdContent = `${response.data}`;
        submit.style.display = 'block';
        responseLabel.innerHTML = 'Response:';
    })
    .catch(error => {
        console.error('Error:', error);
    });
});
