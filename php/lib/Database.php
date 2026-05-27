<?php
declare(strict_types=1);

final class Database
{
    public static function connect(): PDO
    {
        $config = self::loadConfig()['mysql'] ?? [];
        $host = self::env('TELETYPTEL_DB_HOST', (string)($config['host'] ?? '127.0.0.1'));
        $port = (int)self::env('TELETYPTEL_DB_PORT', (string)($config['port'] ?? '3306'));
        $database = self::env('TELETYPTEL_DB_NAME', (string)($config['database'] ?? 'teletyptel'));
        $username = self::env('TELETYPTEL_DB_USER', (string)($config['username'] ?? 'teletyptel'));
        $password = self::env('TELETYPTEL_DB_PASSWORD', (string)($config['password'] ?? ''));
        $charset = (string)($config['charset'] ?? 'utf8mb4');

        $dsn = "mysql:host={$host};port={$port};dbname={$database};charset={$charset}";
        return new PDO($dsn, $username, $password, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            PDO::ATTR_EMULATE_PREPARES => false,
        ]);
    }

    private static function loadConfig(): array
    {
        $path = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'config.php';
        if (!is_file($path)) {
            return [];
        }

        $config = require $path;
        return is_array($config) ? $config : [];
    }

    private static function env(string $key, string $fallback): string
    {
        $value = getenv($key);
        return $value === false || $value === '' ? $fallback : $value;
    }
}
