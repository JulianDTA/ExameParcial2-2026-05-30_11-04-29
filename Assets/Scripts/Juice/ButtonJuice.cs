// ButtonJuice.cs
// Integra el New Input System con feedback de UI y lerp de color de material (instanciado en runtime).
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Juice
{
    [RequireComponent(typeof(UIJuice))]
    public class ButtonJuice : MonoBehaviour
    {
        [Header("Material Color Lerp")]
        [SerializeField] private Renderer buttonRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pressedColor = Color.yellow;
        [SerializeField] private float colorLerpSpeed = 8f;

        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private bool requireHover = true;

        private UIJuice _juice;
        private Material _matInstance; // instancia propia; no toca el .mat compartido
        private Color _targetColor;
        private bool _isHovered;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private bool _useBaseColor;

        void Awake()
        {
            _juice = GetComponent<UIJuice>();
            if (buttonRenderer != null)
            {
                _matInstance = buttonRenderer.material; // Unity clona automáticamente
                _useBaseColor = _matInstance.HasProperty(BaseColorId); // URP usa _BaseColor
                SetColor(normalColor);
            }
            _targetColor = normalColor;
        }

        void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.action.performed += OnInteract;
                interactAction.action.Enable();
            }
        }

        void OnDisable()
        {
            if (interactAction != null)
                interactAction.action.performed -= OnInteract;
        }

        void Update()
        {
            if (_matInstance == null) return;
            Color current = _useBaseColor ? _matInstance.GetColor(BaseColorId) : _matInstance.color;
            SetColor(Color.Lerp(current, _targetColor, colorLerpSpeed * Time.deltaTime));
        }

        private void SetColor(Color c)
        {
            if (_useBaseColor) _matInstance.SetColor(BaseColorId, c);
            else _matInstance.color = c;
        }

        private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (requireHover && !_isHovered) return;
            Trigger();
        }

        // Public para invocar desde otros sistemas (clic UI, etc.)
        public void Trigger()
        {
            _juice.PunchScale();
            _juice.FlashColor();
            _targetColor = pressedColor;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.2f);

            if (AudioManager.Instance != null) AudioManager.Instance.Play("tone");
            if (JuiceManager.Instance != null) JuiceManager.Instance.AddTrauma(0.15f);
        }

        public void OnHoverEnter()
        {
            _isHovered = true;
            _targetColor = Color.Lerp(normalColor, pressedColor, 0.3f);
            _juice.PunchScale();
        }

        public void OnHoverExit()
        {
            _isHovered = false;
            _targetColor = normalColor;
        }

        private void ResetColor() => _targetColor = normalColor;

        void OnDestroy()
        {
            if (_matInstance != null) Destroy(_matInstance);
        }
    }
}
