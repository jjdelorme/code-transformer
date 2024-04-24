const responseArea = document.getElementById('responseArea');
const responseLabel = document.getElementById('responseLabel');
const form = document.getElementById('form');
const submit = document.getElementById('submit');

form.addEventListener('submit', function(event) {
    event.preventDefault(); // Prevent default form submission

    responseLabel.innerHTML = 'Transforming...';
    responseLabel.style.display = 'block';

    submit.style.display = 'none';
    responseArea.mdContent = '';

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
        responseLabel.style.display = 'none';
    })
    .catch(error => {
        console.error('Error:', error);
        responseLabel.style.display = 'none';
        submit.style.display = 'block';
        responseArea.mdContent = `**ERROR**:\n\n${error}`;
    });
});
