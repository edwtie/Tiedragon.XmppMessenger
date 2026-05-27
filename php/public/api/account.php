<?php
declare(strict_types=1);

require_once dirname(__DIR__, 2) . DIRECTORY_SEPARATOR . 'lib' . DIRECTORY_SEPARATOR . 'Database.php';

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store');

try {
    if ($_SERVER['REQUEST_METHOD'] === 'GET') {
        readAccount();
        return;
    }

    if ($_SERVER['REQUEST_METHOD'] === 'POST') {
        saveAccount();
        return;
    }

    http_response_code(405);
    echo json_encode(['ok' => false, 'error' => 'method_not_allowed']);
} catch (Throwable $error) {
    http_response_code(500);
    echo json_encode(['ok' => false, 'error' => 'server_error', 'message' => $error->getMessage()]);
}

function readAccount(): void
{
    $accountId = cleanText($_GET['accountId'] ?? 'local-edward', 96);
    $pdo = Database::connect();
    $statement = $pdo->prepare('SELECT * FROM account_profiles WHERE account_id = :account_id');
    $statement->execute(['account_id' => $accountId]);
    $row = $statement->fetch();

    if (!$row) {
        http_response_code(404);
        echo json_encode(['ok' => false, 'error' => 'not_found']);
        return;
    }

    echo json_encode(['ok' => true, 'account' => rowToAccount($row)], JSON_UNESCAPED_SLASHES);
}

function saveAccount(): void
{
    $input = json_decode(file_get_contents('php://input') ?: '', true);
    if (!is_array($input)) {
        http_response_code(400);
        echo json_encode(['ok' => false, 'error' => 'invalid_json']);
        return;
    }

    $account = normalizeAccount($input);
    $pdo = Database::connect();
    $statement = $pdo->prepare(
        'INSERT INTO account_profiles (
            account_id, jid, display_name, password_secret, remember_password,
            phone_number, provider_id, accessibility_profile_id, preferred_language,
            relay_websocket, xmpp_websocket, peer
        ) VALUES (
            :account_id, :jid, :display_name, :password_secret, :remember_password,
            :phone_number, :provider_id, :accessibility_profile_id, :preferred_language,
            :relay_websocket, :xmpp_websocket, :peer
        )
        ON DUPLICATE KEY UPDATE
            jid = VALUES(jid),
            display_name = VALUES(display_name),
            password_secret = VALUES(password_secret),
            remember_password = VALUES(remember_password),
            phone_number = VALUES(phone_number),
            provider_id = VALUES(provider_id),
            accessibility_profile_id = VALUES(accessibility_profile_id),
            preferred_language = VALUES(preferred_language),
            relay_websocket = VALUES(relay_websocket),
            xmpp_websocket = VALUES(xmpp_websocket),
            peer = VALUES(peer)'
    );
    $statement->execute($account);

    unset($account['password_secret']);
    echo json_encode(['ok' => true, 'account' => accountToClient($account)], JSON_UNESCAPED_SLASHES);
}

function normalizeAccount(array $input): array
{
    $rememberPassword = ($input['rememberPassword'] ?? false) === true;

    return [
        'account_id' => cleanText($input['accountId'] ?? 'local-account', 96),
        'jid' => cleanText($input['jid'] ?? '', 255),
        'display_name' => cleanText($input['displayName'] ?? 'Me', 120),
        'password_secret' => $rememberPassword ? cleanText($input['password'] ?? '', 1024) : '',
        'remember_password' => $rememberPassword ? 1 : 0,
        'phone_number' => cleanText($input['phoneNumber'] ?? '', 64),
        'provider_id' => cleanText($input['providerId'] ?? 'example-provider', 96),
        'accessibility_profile_id' => cleanText($input['accessibilityProfileId'] ?? 'default-live-text', 96),
        'preferred_language' => cleanText($input['preferredLanguage'] ?? 'nl', 16),
        'relay_websocket' => cleanText($input['relayWebSocket'] ?? 'ws://127.0.0.1:8787', 255),
        'xmpp_websocket' => cleanText($input['xmppWebSocket'] ?? 'ws://127.0.0.1:8787', 255),
        'peer' => cleanText($input['peer'] ?? 'relay@localhost', 255),
    ];
}

function rowToAccount(array $row): array
{
    return [
        'accountId' => $row['account_id'],
        'jid' => $row['jid'],
        'displayName' => $row['display_name'],
        'rememberPassword' => (bool)$row['remember_password'],
        'password' => (bool)$row['remember_password'] ? (string)$row['password_secret'] : '',
        'phoneNumber' => $row['phone_number'],
        'providerId' => $row['provider_id'],
        'accessibilityProfileId' => $row['accessibility_profile_id'],
        'preferredLanguage' => $row['preferred_language'],
        'relayWebSocket' => $row['relay_websocket'],
        'xmppWebSocket' => $row['xmpp_websocket'],
        'peer' => $row['peer'],
        'savedInDatabase' => true,
    ];
}

function accountToClient(array $account): array
{
    return [
        'accountId' => $account['account_id'],
        'jid' => $account['jid'],
        'displayName' => $account['display_name'],
        'rememberPassword' => (bool)$account['remember_password'],
        'phoneNumber' => $account['phone_number'],
        'providerId' => $account['provider_id'],
        'accessibilityProfileId' => $account['accessibility_profile_id'],
        'preferredLanguage' => $account['preferred_language'],
        'relayWebSocket' => $account['relay_websocket'],
        'xmppWebSocket' => $account['xmpp_websocket'],
        'peer' => $account['peer'],
        'savedInDatabase' => true,
    ];
}

function cleanText(mixed $value, int $maxLength): string
{
    $text = trim((string)$value);
    if (function_exists('mb_substr')) {
        return mb_substr($text, 0, $maxLength, 'UTF-8');
    }

    return substr($text, 0, $maxLength);
}
