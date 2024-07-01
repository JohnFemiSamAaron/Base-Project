using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Bullet _bullet;
    [SerializeField] private TurretBullet _turretBullet;
    [SerializeField] private Canon _canon;
    [SerializeField] private DropBox _dropBox;

    private PlayerMovement _playerMovement;
    private PlayerAiming _playerAiming;
    private PlayerHealth _playerHealth;
    private PowerUpManager _powerUpManager;

    private PowerUpManager.Powerups _currentActivePowerUp;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAiming = GetComponent<PlayerAiming>();
        _playerHealth = GetComponent<PlayerHealth>();
        _powerUpManager = GetComponent<PowerUpManager>();
    }

    private void Start()
    {
        if (_powerUpManager != null)
        {
            _powerUpManager.OnPowerUpChanged += HandlePowerUpChanged;
            _currentActivePowerUp = _powerUpManager.GetCurrentActivePowerUp();
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        _playerMovement.MoveLocal(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _playerAiming.Aim();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireBullet();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            UseActivePowerUp();
        }
    }

    private void FireBullet()
    {
        Bullet _bulletToFire = Instantiate(_bullet, _spawnPoint.position, Quaternion.identity);
        _bulletToFire.Shoot(_playerAiming.turn);
        _bulletToFire.Owner = gameObject;
    }

    private void UseActivePowerUp()
    {
        switch (_currentActivePowerUp)
        {
            case PowerUpManager.Powerups.Turret:
                TurretBullet _firedBullet = Instantiate(_turretBullet, _spawnPoint.position, Quaternion.identity);
                _firedBullet.Shoot(_playerAiming.turn);
                _firedBullet.Owner = gameObject;
                break;

            case PowerUpManager.Powerups.Canon:
                Instantiate(_canon, _spawnPoint.position, Quaternion.identity);
                break;

            case PowerUpManager.Powerups.DropBox:
                Instantiate(_dropBox, _spawnPoint.position, transform.rotation);
                break;

            case PowerUpManager.Powerups.Regen:
                // Do nothing
                break;

            case PowerUpManager.Powerups.Juggernaut:
                // Do nothing
                break;

            default:
                Debug.LogWarning("No valid power-up selected");
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Projectile _projectile = collision.gameObject.GetComponent<Projectile>();

        if (_projectile != null)
        {
            // Check if the projectile was fired by the player
            if (_projectile.Owner != gameObject)
            {
                float _damagePoints = _projectile.GetDamagePoints();
                _playerHealth.TakeDamage(_damagePoints);
            }

            Destroy(collision.gameObject); // Destroy the projectile after collision
            return;
        }

        // Hardcoding the damage points of turret, canon because of the deadline
        switch (collision.gameObject.name)
        {
            case "Turret Bullet":
                _playerHealth.TakeDamage(7);
                break;

            case "CannonBullet":
                _playerHealth.TakeDamage(10);
                break;

            default:
                break;
        }

        Destroy(collision.gameObject);
    }


    private void HandlePowerUpChanged(PowerUpManager.Powerups newPowerUp)
    {
        switch (newPowerUp)
        {
            case PowerUpManager.Powerups.Regen:
                _currentActivePowerUp = PowerUpManager.Powerups.Regen;
                break;

            case PowerUpManager.Powerups.Juggernaut:
                _currentActivePowerUp = PowerUpManager.Powerups.Juggernaut;
                break;

            case PowerUpManager.Powerups.Turret:
                _currentActivePowerUp = PowerUpManager.Powerups.Turret;
                break;

            case PowerUpManager.Powerups.Canon:
                _currentActivePowerUp = PowerUpManager.Powerups.Canon;
                break;

            case PowerUpManager.Powerups.DropBox:
                _currentActivePowerUp = PowerUpManager.Powerups.DropBox;
                break;

            default:
                Debug.LogWarning("No valid power-up selected");
                break;
        }
    }

    private void OnDisable()
    {
        if (_powerUpManager != null)
        {
            _powerUpManager.OnPowerUpChanged -= HandlePowerUpChanged;
        }
    }
}
