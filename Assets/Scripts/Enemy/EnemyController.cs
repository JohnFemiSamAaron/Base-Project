using Org.BouncyCastle.Asn1.Esf;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Bullet _bullet;
    [SerializeField] private TurretBullet _turretBullet;
    [SerializeField] private Canon _canon;
    [SerializeField] private DropBox _dropBox;

    private PlayerMovement _playerMovement;
    private PlayerAiming _playerAiming;
    private EnemyHealth _enemyHealth;
    private PowerUpManager _powerUpManager;

    private PowerUpManager.Powerups _currentActivePowerUp;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAiming = GetComponent<PlayerAiming>();
        _enemyHealth = GetComponent<EnemyHealth>();
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


    private void FireBullet()
    {
        Instantiate(_bullet, _spawnPoint.position, Quaternion.identity);
    }

    private void UseActivePowerUp()
    {
        switch (_currentActivePowerUp)
        {
            case PowerUpManager.Powerups.Turret:
                TurretBullet _firedBullet = Instantiate(_turretBullet, _spawnPoint.position, Quaternion.identity);
                _firedBullet.Shoot(_playerAiming.turn);
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
            float _damagePoints = _projectile.GetDamagePoints();
            _enemyHealth.TakeDamage(_damagePoints);
            return;
        }

        // basically hardcoding the damage points of turret, canon because of the deadline
        switch (collision.gameObject.name)
        {
            case "Turret Bullet":
                _enemyHealth.TakeDamage(7);
                break;

            case "CannonBullet":
                _enemyHealth.TakeDamage(10);
                break;

            default:
                break;
        }
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
