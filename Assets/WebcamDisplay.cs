using UnityEngine;

public class WebcamDisplay : MonoBehaviour
{
    private WebCamTexture webcamTexture; // Webcam feed
    public GameObject trackedObject; // The cube to move
    public Color targetColor = Color.red; // The color to track
    public float colorThreshold = 0.2f; // Sensitivity for color matching
    public float movementSmoothing = 5f; // Speed of smoothing for cube movement

    void Start()
    {
        
        webcamTexture = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }

    void Update()
    {
        if (webcamTexture.isPlaying)
        {
            
            Color[] pixels = webcamTexture.GetPixels();
            Vector2 averagePosition = Vector2.zero;
            int colorCount = 0;

            // Loop through the pixels to find the target color
            for (int y = 0; y < webcamTexture.height; y++)
            {
                for (int x = 0; x < webcamTexture.width; x++)
                {
                    Color pixelColor = pixels[y * webcamTexture.width + x];

                    if (IsColorMatch(pixelColor, targetColor, colorThreshold))
                    {
                        averagePosition += new Vector2(x, y);
                        colorCount++;
                    }
                }
            }

            // Calculate the average position of the target color
            if (colorCount > 0)
            {
                averagePosition /= colorCount;

                // Normalize the position to viewport space
                float xNormalized = averagePosition.x / webcamTexture.width;
                float yNormalized = averagePosition.y / webcamTexture.height;

                // Clamp values to ensure the cube stays within the screen
                xNormalized = Mathf.Clamp(xNormalized, 0f, 1f);
                yNormalized = Mathf.Clamp(yNormalized, 0f, 1f);

                // Convert normalized coordinates to world space
                Vector3 viewportPosition = new Vector3(xNormalized, yNormalized, 2f); // Place the cube closer

                Vector3 worldPosition = Camera.main.ViewportToWorldPoint(viewportPosition);

                // Smoothly move the cube to the target position
                trackedObject.transform.position = Vector3.Lerp(
                    trackedObject.transform.position,
                    worldPosition,
                    Time.deltaTime * movementSmoothing
                );
            }
        }
    }

    private bool IsColorMatch(Color pixelColor, Color targetColor, float threshold)
    {
        // Check if the pixel color is within the threshold of the target color
        return Mathf.Abs(pixelColor.r - targetColor.r) < threshold &&
               Mathf.Abs(pixelColor.g - targetColor.g) < threshold &&
               Mathf.Abs(pixelColor.b - targetColor.b) < threshold;
    }

    void OnDestroy()
    {
        // Stop the webcam when the object is destroyed
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}
