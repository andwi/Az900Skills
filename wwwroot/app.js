(function () {
    const loader = document.getElementById('loader');
    const snackbar = document.getElementById('snackbar');

    function showSnackbar(text, bgColor) {
        snackbar.innerHTML = text;
        snackbar.style.backgroundColor = bgColor;
        snackbar.classList.add('show');
        setTimeout(() => snackbar.classList.remove('show'), 3000);
    }

    const app = Vue.createApp({
        data() {
            return {
                json: null,
                saving: false,
            };
        },
        mounted() {
            this.fetchSkills();
        },
        methods: {
            fetchSkills() {
                setTimeout(() => {
                    if (!this.json) {
                        loader.classList.add('show');
                    }
                }, 200);

                axios.get('/skills')
                    .then(response => {
                        this.json = response.data;
                    })
                    .catch(error => {
                        console.error('Error fetching skills:', error);
                        showSnackbar('Error fetching skills', 'darkred');
                    })
                    .finally(() => {
                        loader.classList.remove('show');
                    });
            },
            saveSkills() {
                this.saving = true;

                axios.post('/skills', this.json)
                    .then(response => {
                        console.log('Skills saved successfully:', response.data);
                        showSnackbar('Skills saved successfully', 'green');
                    })
                    .catch(error => {
                        console.error('Error saving skills:', error);
                        showSnackbar('Error saving skills', 'darkred');
                    })
                    .finally(() => {
                        this.saving = false;
                    });
            }
        }
    });

    app.mount('#app');
})();
