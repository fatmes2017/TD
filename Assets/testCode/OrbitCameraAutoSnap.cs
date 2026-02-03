using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class OrbitCameraAutoSnap : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Orbit Settings")]
    public float distance = 5.0f;
    public float rotationSpeed = 2.0f;
    public float zoomSpeed = 0.5f;
    public float minDistance = 2.0f;
    public float maxDistance = 15.0f;

    [Header("Vertical Limits")]
    public float minVerticalAngle = -20.0f;
    public float maxVerticalAngle = 80.0f;

    [Header("Auto-Snap Settings")]
    public bool enableAutoSnap = true;
    public float snapRadius = 15.0f; // شعاع جذب (درجه)
    public float snapStrength = 5.0f; // قدرت جذب
    public float snapDistanceThreshold = 1.0f; // آستانه فاصله برای اسنپ کامل
    public List<SnapPoint> snapPoints = new List<SnapPoint>();

    [Header("Magnetic Snap (Like a Magnet)")]
    public bool useMagneticSnap = true; // مانند آهنربا جذب کند
    public float magneticForce = 10.0f; // قدرت مغناطیسی

    [Header("Visual Feedback")]
    public bool showSnapVisuals = true;
    public Color snapZoneColor = new Color(1, 0.5f, 0, 0.3f); // رنگ ناحیه اسنپ
    public Color activeSnapColor = Color.green; // رنگ وقتی فعال است

    [Header("Smoothness")]
    public bool smoothRotation = true;
    public float smoothTime = 0.3f;

    [Header("Mobile Settings")]
    public float mobileRotationMultiplier = 0.5f;
    public float mobileZoomMultiplier = 0.01f;
    public bool enablePinchZoom = true;
    public bool enableDragRotation = true;

    [System.Serializable]
    public class SnapPoint
    {
        public string name = "Snap Point";
        public float yawAngle = 0f;
        public float pitchAngle = 30f;
        public float snapDistance = 5f;
        public KeyCode hotkey = KeyCode.None;
        [Range(0.1f, 50f)] public float attractionRadius = 15f; // شعاع جذب برای این نقطه خاص
        [Range(0.1f, 20f)] public float attractionStrength = 5f; // قدرت جذب برای این نقطه خاص
    }

    // متغیرهای داخلی
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private Quaternion currentRotation;

    // برای اسنپ
    private int currentSnapIndex = -1;
    private float snapProgress = 0f;
    private bool isBeingSnapped = false;
    private Vector2 snapVelocity = Vector2.zero;

    // متغیرهای موبایل
    private Vector2?[] oldTouchPositions = { null, null };
    private float oldTouchDistance;
    private Vector2 touchStartPosition;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("لطفا یک هدف برای دوربین تنظیم کنید!");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
        currentRotation = transform.rotation;

        // ایجاد نقاط اسنپ پیش‌فرض
        if (snapPoints.Count == 0)
        {
            CreateDefaultSnapPoints();
        }
    }

    void CreateDefaultSnapPoints()
    {
        snapPoints.Add(new SnapPoint()
        {
            name = "Front",
            yawAngle = 0f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha1,
            attractionRadius = 15f,
            attractionStrength = 5f
        });

        snapPoints.Add(new SnapPoint()
        {
            name = "Right",
            yawAngle = 90f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha2,
            attractionRadius = 15f,
            attractionStrength = 5f
        });

        snapPoints.Add(new SnapPoint()
        {
            name = "Back",
            yawAngle = 180f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha3,
            attractionRadius = 15f,
            attractionStrength = 5f
        });

        snapPoints.Add(new SnapPoint()
        {
            name = "Left",
            yawAngle = 270f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha4,
            attractionRadius = 15f,
            attractionStrength = 5f
        });
    }

    void Update()
    {
        if (target == null) return;

        // کنترل کیبورد برای اسنپ فوری
        HandleKeyboardSnap();

        // تشخیص پلتفرم
        if (Application.isMobilePlatform || IsTouchDevice())
        {
            HandleMobileInput();
        }
        else
        {
            HandleDesktopInput();
        }

        // پردازش اسنپ خودکار در حین حرکت
        if (enableAutoSnap)
        {
            ProcessAutoSnap();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        UpdateCameraPosition();
    }

    void HandleDesktopInput()
    {
        if (Input.GetMouseButton(0))
        {
            // حرکت دوربین با ماوس
            float deltaX = Input.GetAxis("Mouse X") * rotationSpeed;
            float deltaY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // اگر در حال اسنپ هستیم، بررسی کن آیا کاربر می‌خواهد از اسنپ خارج شود
            if (isBeingSnapped && (Mathf.Abs(deltaX) > 0.1f || Mathf.Abs(deltaY) > 0.1f))
            {
                // اگر کاربر به اندازه کافی حرکت داد، از اسنپ خارج شو
                float escapeForce = new Vector2(deltaX, deltaY).magnitude;
                if (escapeForce > 0.5f)
                {
                    isBeingSnapped = false;
                    currentSnapIndex = -1;
                    snapVelocity = Vector2.zero;
                }
            }

            // اعمال حرکت
            currentX += deltaX;
            currentY -= deltaY;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        // کنترل اسکرول
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * 50f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void HandleMobileInput()
    {
        if (enableDragRotation && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float deltaX = (touch.position.x - touchStartPosition.x) * 0.01f * rotationSpeed * mobileRotationMultiplier;
                float deltaY = (touch.position.y - touchStartPosition.y) * 0.01f * rotationSpeed * mobileRotationMultiplier;

                // اگر در حال اسنپ هستیم، بررسی کن آیا کاربر می‌خواهد از اسنپ خارج شود
                if (isBeingSnapped && (Mathf.Abs(deltaX) > 0.05f || Mathf.Abs(deltaY) > 0.05f))
                {
                    float escapeForce = new Vector2(deltaX, deltaY).magnitude;
                    if (escapeForce > 0.3f)
                    {
                        isBeingSnapped = false;
                        currentSnapIndex = -1;
                        snapVelocity = Vector2.zero;
                    }
                }

                currentX += deltaX;
                currentY -= deltaY;
                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

                touchStartPosition = touch.position;
            }
        }

        // کنترل زوم با پینچ
        if (enablePinchZoom && Input.touchCount == 2)
        {
            HandlePinchZoom();
        }
    }

    void HandlePinchZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            oldTouchDistance = Vector2.Distance(touch1.position, touch2.position);
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float newTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            float deltaDistance = (oldTouchDistance - newTouchDistance) * zoomSpeed * mobileZoomMultiplier;

            distance += deltaDistance;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            oldTouchDistance = newTouchDistance;
        }
    }

    void HandleKeyboardSnap()
    {
        // کلیدهای میانبر برای اسنپ فوری
        for (int i = 0; i < snapPoints.Count; i++)
        {
            if (Input.GetKeyDown(snapPoints[i].hotkey))
            {
                ForceSnapToPoint(i);
                break;
            }
        }
    }

    void ProcessAutoSnap()
    {
        if (isBeingSnapped && currentSnapIndex != -1)
        {
            // اگر در حال اسنپ هستیم، به سمت نقطه هدف حرکت کن
            SnapPoint targetPoint = snapPoints[currentSnapIndex];

            // محاسبه اختلاف زاویه
            float yawDiff = Mathf.DeltaAngle(currentX, targetPoint.yawAngle);
            float pitchDiff = Mathf.DeltaAngle(currentY, targetPoint.pitchAngle);

            // اگر به اندازه کافی نزدیک شدیم، اسنپ کامل شود
            if (Mathf.Abs(yawDiff) < snapDistanceThreshold && Mathf.Abs(pitchDiff) < snapDistanceThreshold)
            {
                currentX = targetPoint.yawAngle;
                currentY = targetPoint.pitchAngle;
                distance = targetPoint.snapDistance;
            }
            else
            {
                // حرکت نرم به سمت نقطه اسنپ
                currentX = Mathf.SmoothDampAngle(currentX, targetPoint.yawAngle, ref snapVelocity.x, 0.1f);
                currentY = Mathf.SmoothDampAngle(currentY, targetPoint.pitchAngle, ref snapVelocity.y, 0.1f);
                distance = Mathf.Lerp(distance, targetPoint.snapDistance, Time.deltaTime * 2f);
            }
        }
        else
        {
            // اگر در حال اسنپ نیستیم، بررسی کن آیا به نقطه‌ای نزدیک شده‌ایم
            FindNearestSnapPoint();
        }
    }

    void FindNearestSnapPoint()
    {
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;
        float currentStrength = 0f;

        for (int i = 0; i < snapPoints.Count; i++)
        {
            SnapPoint point = snapPoints[i];

            // محاسبه فاصله زاویه‌ای
            float yawDiff = Mathf.DeltaAngle(currentX, point.yawAngle);
            float pitchDiff = Mathf.DeltaAngle(currentY, point.pitchAngle);
            float angularDistance = Mathf.Sqrt(yawDiff * yawDiff + pitchDiff * pitchDiff);

            // اگر در شعاع جذب هستیم
            float effectiveRadius = point.attractionRadius;
            if (angularDistance < effectiveRadius)
            {
                // محاسبه قدرت جذب (نزدیک‌تر = قوی‌تر)
                float attractionFactor = 1f - (angularDistance / effectiveRadius);

                if (useMagneticSnap)
                {
                    // محاسبه نیروی مغناطیسی
                    float magneticPull = attractionFactor * point.attractionStrength * magneticForce * Time.deltaTime;

                    // اعمال نیروی جذب
                    float pullX = -yawDiff * magneticPull;
                    float pullY = -pitchDiff * magneticPull;

                    currentX += pullX;
                    currentY += pullY;

                    // اگر خیلی نزدیک شدیم، اسنپ کامل کنیم
                    if (angularDistance < 2f)
                    {
                        isBeingSnapped = true;
                        currentSnapIndex = i;
                        snapVelocity = Vector2.zero;
                    }
                }
                else
                {
                    // روش ساده: اگر نزدیک‌ترین نقطه است، ذخیره کن
                    if (angularDistance < nearestDistance)
                    {
                        nearestDistance = angularDistance;
                        nearestIndex = i;
                        currentStrength = point.attractionStrength;
                    }
                }
            }
        }

        // اگر نقطه‌ای پیدا شد و از روش مغناطیسی استفاده نمی‌کنیم
        if (!useMagneticSnap && nearestIndex != -1)
        {
            // محاسبه قدرت جذب بر اساس فاصله
            float pullStrength = (1f - (nearestDistance / snapPoints[nearestIndex].attractionRadius)) *
                                snapPoints[nearestIndex].attractionStrength * Time.deltaTime;

            // اعمال جذب
            SnapPoint point = snapPoints[nearestIndex];
            float yawDiff = Mathf.DeltaAngle(currentX, point.yawAngle);
            float pitchDiff = Mathf.DeltaAngle(currentY, point.pitchAngle);

            currentX += -yawDiff * pullStrength;
            currentY += -pitchDiff * pullStrength;

            // اگر خیلی نزدیک شد، اسنپ کامل کن
            if (nearestDistance < 2f)
            {
                isBeingSnapped = true;
                currentSnapIndex = nearestIndex;
                snapVelocity = Vector2.zero;
            }
        }
    }

    void ForceSnapToPoint(int index)
    {
        if (index < 0 || index >= snapPoints.Count) return;

        currentSnapIndex = index;
        isBeingSnapped = true;
        snapVelocity = Vector2.zero;

        // اسنپ فوری
        SnapPoint point = snapPoints[index];
        currentX = point.yawAngle;
        currentY = point.pitchAngle;
        distance = point.snapDistance;
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        if (smoothRotation && !isBeingSnapped)
        {
            currentRotation = Quaternion.Slerp(currentRotation, rotation,
                smoothTime * Time.deltaTime * 10f);
        }
        else
        {
            currentRotation = rotation;
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = currentRotation * negDistance + target.position;

        transform.rotation = currentRotation;
        transform.position = position;
        transform.LookAt(target);
    }

    // متدهای عمومی
    public void AddSnapPoint(float yaw, float pitch, float snapDistance, string name = "New Point")
    {
        snapPoints.Add(new SnapPoint()
        {
            name = name,
            yawAngle = yaw,
            pitchAngle = pitch,
            snapDistance = snapDistance,
            attractionRadius = snapRadius,
            attractionStrength = snapStrength
        });
    }

    public void SnapToPointByName(string pointName)
    {
        for (int i = 0; i < snapPoints.Count; i++)
        {
            if (snapPoints[i].name == pointName)
            {
                ForceSnapToPoint(i);
                return;
            }
        }
    }

    bool IsTouchDevice()
    {
        return Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer;
    }

    // برای نمایش در ادیتور
    void OnDrawGizmos()
    {
        if (!showSnapVisuals || target == null) return;

        // نمایش نقاط اسنپ و ناحیه جذب آنها
        foreach (var point in snapPoints)
        {
            // محاسبه موقعیت نقطه
            Quaternion rotation = Quaternion.Euler(point.pitchAngle, point.yawAngle, 0);
            Vector3 position = target.position + rotation * Vector3.back * point.snapDistance;

            // رنگ بر اساس فعال بودن
            Color zoneColor = (isBeingSnapped && snapPoints.IndexOf(point) == currentSnapIndex) ?
                activeSnapColor : snapZoneColor;

            // رسم کره ناحیه جذب
            Gizmos.color = zoneColor;
            Gizmos.DrawWireSphere(position, point.attractionRadius * 0.1f);

            // رسم خط به هدف
            Gizmos.color = zoneColor;
            Gizmos.DrawLine(target.position, position);

            // نمایش نام
#if UNITY_EDITOR
            UnityEditor.Handles.Label(position + Vector3.up * 0.3f, point.name);
#endif
        }
    }

    void OnGUI()
    {
        if (showSnapVisuals && isBeingSnapped && currentSnapIndex != -1)
        {
            // نمایش پیغام اسنپ
            GUI.Box(new Rect(10, 10, 200, 25), $"Snapped to: {snapPoints[currentSnapIndex].name}");
        }
    }
}
