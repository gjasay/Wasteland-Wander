﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 3.5f;
    [SerializeField]
    private float _speedMultiplier = 2f;
    [SerializeField]
    private GameObject _bulletPrefab;
    [SerializeField]
    private GameObject _tripleShotPrefab;
    [SerializeField]
    private float _defaultFireRate = 0.15f;
    [SerializeField]
    private float _tripleShotFireRate = 0.5f;
    private float _canFire = -0.1f;
    [SerializeField]
    private int _health = 100;
    [SerializeField]
    private float _bulletOffset = 1.5f;
    [SerializeField]
    private GameObject _shieldVisualizer;
    [SerializeField]
    private GameObject _leftEngine, _rightEngine;
    [SerializeField]
    private int _score;
    [SerializeField]
    private UIManager _uiManager;
    [SerializeField]
    private int _damagePerHit;
    [SerializeField]
    private float _turnMaxAngle;
    [SerializeField]
    private AudioSource _audioSource;
    private float r;
    private bool movingLeft;
    private bool movingRight;

    private bool _isTripleShotActive = false;
    private bool _isSpeedBoostActive = false;
    private bool _isShieldActive = false;
    private SpawnManager _spawnManager;



    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);

        _spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("UI").GetComponent<UIManager>();
        _audioSource = GetComponent<AudioSource>();

        if ( _uiManager == null )
        {
            Debug.LogError("The UI Manager is null");
        }

        if (_spawnManager == null )
        {
            Debug.LogError("The Spawn Manager is null");
        }
        if (_audioSource == null )
        {
            Debug.LogError("Player audio source is null");
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();

        if (Input.GetKey(KeyCode.Space) && Time.time > _canFire)
        {
            FireBullet();
        }

    }
    void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        transform.Translate((_isSpeedBoostActive ? _speedMultiplier * _speed : _speed) * Time.deltaTime * new Vector3(horizontalInput * 2.5f, verticalInput, 0));

        // Restricts bound on Y Axis
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 4.5f), 0);

        // Wrapping on the X axis
        if (transform.position.x > 9.4f)
        {
            transform.position = new Vector3(-9.4f, transform.position.y, 0);
        }
        else if (transform.position.x < -9.4f)
        {
            transform.position = new Vector3(9.4f, transform.position.y, 0);
        }

        //Rotation
        if (Input.GetKey(KeyCode.A))
        {
            movingLeft = true;
            movingRight = false;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movingLeft = false;
            movingRight = true;
        }
        else
        {
            movingLeft = false;
            movingRight = false;
        }

        float targetAngle = 0;
        if (movingLeft && !movingRight)
        {
            targetAngle = _turnMaxAngle;
        }
        else if (movingRight && !movingLeft)
        {
            targetAngle = _turnMaxAngle * -1;
        }
        else
        {
            targetAngle = 0;
        }
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref r, 0.25f);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void FireBullet()
    {
        _canFire = Time.time + (_isTripleShotActive ? _tripleShotFireRate : _defaultFireRate);

        Vector3 bulletPosition = new Vector3(transform.position.x, transform.position.y + _bulletOffset, 0);

        if (_isTripleShotActive)
        {
            Instantiate(_tripleShotPrefab, transform.position, transform.rotation);
        }
        else
        {
            Instantiate(_bulletPrefab, bulletPosition, transform.rotation * transform.rotation);
        }

        _audioSource.Play();
    }

    public void Damage()
    {
        if (!_isShieldActive)
        {
            _health -= _damagePerHit;
            _uiManager.UpdateHealth(_health);
            if (_health <= 66)
            {
                _leftEngine.SetActive(true);
            }
            if (_health <= 33)
            {
                _rightEngine.SetActive(true);
            }
            if (_health <= 0)
            {
                _health = 0;
                _spawnManager.onPlayerDeath();
                Destroy(gameObject);
                _uiManager.GameOverSequence();
            }
        } else
        {
            _isShieldActive = false;

            if (_shieldVisualizer != null)
            {
                _shieldVisualizer.SetActive(false);
            }

            return;
        }
    }

    public IEnumerator TripleShotRoutine(float tripleShotDuration)
    {    
        if (_isTripleShotActive)
        {
            yield return new WaitUntil(() => !_isTripleShotActive);
        }

        _isTripleShotActive = true;
        yield return new WaitForSeconds(tripleShotDuration);
        _isTripleShotActive = false;

    }

    public IEnumerator SpeedBoostRoutine(float speedBoostDuration)
    {
        if (_isSpeedBoostActive)
        {
            yield return new WaitUntil(() => !_isSpeedBoostActive);
        }

        _isSpeedBoostActive = true;
        yield return new WaitForSeconds(speedBoostDuration);
        _isSpeedBoostActive = false;

    }

    public void ActivateShield()
    {
        _isShieldActive = true;
        if (_shieldVisualizer != null)
        {
            _shieldVisualizer.SetActive(true);
        }
    }

    public void AddScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }
}