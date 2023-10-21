const picker = document.getElementById('picker');
picker.onchange = () => {
    for (var i = 0; i < picker.files.length; i++) {
        const startTime = new Date();
        const file = picker.files[i];
        const upload = UpChunk.createUpload({
            endpoint: '/upload/' + file.name,
            file,
            chunkSize: 100 * 1024,//Kb * 1024 to get azure size
            maxChunkSize: 1000 * 1024,//Kb * 1024 to get azure size
            dynamicChunkSize: true,
        });

        // subscribe to events
        upload.on('error', err => {
            console.error(`${file.name}: It all went wrong!`, err.detail);
        });

        upload.on('progress', ({ detail: progress }) => {
            console.log(`${file.name}: Progress: ${progress}%`);
        });

        upload.on('attempt', ({ detail }) => {
            console.log(`${file.name}: There was an attempt!`, detail);
        });

        upload.on('attemptFailure', ({ detail }) => {
            console.log(`${file.name}: The attempt failed!`, detail);
        });

        upload.on('chunkSuccess', ({ detail }) => {
            console.log(`${file.name}: Chunk successfully uploaded!`, detail);
        });

        upload.on('success', () => {
            endTime = new Date();
            var seconds = Math.round((endTime - startTime) / 1000);
            console.log(`${file.name}: We did it! in ${seconds} seconds`);
        });
    }
};
