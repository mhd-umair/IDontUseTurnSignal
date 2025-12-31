// Audio Recording Module
window.audioRecorder = {
    mediaRecorder: null,
    audioChunks: [],
    stream: null,

    start: async function () {
        try {
            this.audioChunks = [];
            this.stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            
            // Try to use webm format, fallback to other formats
            const mimeTypes = [
                'audio/webm;codecs=opus',
                'audio/webm',
                'audio/ogg;codecs=opus',
                'audio/mp4',
                'audio/wav'
            ];
            
            let mimeType = '';
            for (const type of mimeTypes) {
                if (MediaRecorder.isTypeSupported(type)) {
                    mimeType = type;
                    break;
                }
            }
            
            const options = mimeType ? { mimeType } : {};
            this.mediaRecorder = new MediaRecorder(this.stream, options);
            
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.audioChunks.push(event.data);
                }
            };

            this.mediaRecorder.start(100); // Collect data every 100ms
            console.log('Recording started with mime type:', mimeType || 'default');
            return true;
        } catch (error) {
            console.error('Error starting recording:', error);
            throw error;
        }
    },

    stop: async function () {
        return new Promise((resolve, reject) => {
            try {
                if (!this.mediaRecorder || this.mediaRecorder.state === 'inactive') {
                    resolve(new Uint8Array(0));
                    return;
                }

                this.mediaRecorder.onstop = async () => {
                    try {
                        const audioBlob = new Blob(this.audioChunks, { type: this.mediaRecorder.mimeType || 'audio/webm' });
                        const arrayBuffer = await audioBlob.arrayBuffer();
                        const uint8Array = new Uint8Array(arrayBuffer);
                        
                        // Stop all tracks
                        if (this.stream) {
                            this.stream.getTracks().forEach(track => track.stop());
                        }
                        
                        console.log('Recording stopped, audio size:', uint8Array.length);
                        resolve(uint8Array);
                    } catch (error) {
                        reject(error);
                    }
                };

                this.mediaRecorder.stop();
            } catch (error) {
                console.error('Error stopping recording:', error);
                reject(error);
            }
        });
    }
};

// Geolocation Module
window.getLocation = function () {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            resolve({
                success: false,
                errorMessage: 'Geolocation is not supported by this browser'
            });
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                resolve({
                    success: true,
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude
                });
            },
            (error) => {
                let errorMessage;
                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        errorMessage = 'Location permission denied. Please enable location access.';
                        break;
                    case error.POSITION_UNAVAILABLE:
                        errorMessage = 'Location information is unavailable.';
                        break;
                    case error.TIMEOUT:
                        errorMessage = 'Location request timed out.';
                        break;
                    default:
                        errorMessage = 'An unknown error occurred.';
                        break;
                }
                resolve({
                    success: false,
                    errorMessage: errorMessage
                });
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    });
};
