const picker = document.getElementById('picker');
picker.onchange = () => {
    for (var i = 0; i < picker.files.length; i++) {
        const file = picker.files[i];
        const upload = UpChunk.createUpload({
            endpoint: '/upload/' + file.name,
            file,
            chunkSize: 30720,
            dynamicChunkSize: false,
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
            console.log(`${file.name}: We did it!`);
        });
    }
};
