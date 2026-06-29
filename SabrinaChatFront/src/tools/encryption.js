import forge from "node-forge";

// Decrypts an encrypted PKCS#8 PEM private key using the password
export async function decryptEncryptedPrivateKey(pem, password) {
  return new Promise((resolve, reject) => {
    try {
      if (!pem || !password) {
        return reject(new Error("Missing PEM or password."));
      }

      const encryptedPrivateKeyInfo = forge.pki.encryptedPrivateKeyFromPem(pem);

      const privateKeyInfo = forge.pki.decryptPrivateKeyInfo(
        encryptedPrivateKeyInfo,
        password,
      );
      if (!privateKeyInfo)
        return reject(new Error("❌ Incorrect password or corrupt PEM."));

      const der = forge.asn1.toDer(privateKeyInfo).getBytes();

      const buffer = new Uint8Array(der.length);
      for (let i = 0; i < der.length; i++) {
        buffer[i] = der.charCodeAt(i);
      }
      crypto.subtle
        .importKey(
          "pkcs8",
          buffer.buffer,
          { name: "RSA-OAEP", hash: "SHA-256" },
          true,
          ["decrypt"],
        )
        .then(resolve)
        .catch((e) =>
          reject(new Error("WebCrypto import failed: " + e.message)),
        );
    } catch (e) {
      reject(new Error("Unexpected error: " + e.message));
    }
  });
}

// Decrypts data encrypted with RSA-OAEP
export async function decryptRSA(base64Data, privateKey) {
  const encryptedBytes = Uint8Array.from(atob(base64Data), (c) =>
    c.charCodeAt(0),
  );
  const decrypted = await window.crypto.subtle.decrypt(
    { name: "RSA-OAEP" },
    privateKey,
    encryptedBytes,
  );
  return decrypted;
}

// Decrypts AES-GCM encrypted message
export async function decryptAESGCM(base64Message, aesKey) {
  const raw = Uint8Array.from(atob(base64Message), (c) => c.charCodeAt(0));
  const iv = raw.slice(0, 12); // First 12 bytes
  const ciphertext = raw.slice(12); // Remaining

  const decrypted = await crypto.subtle.decrypt(
    {
      name: "AES-GCM",
      iv: iv,
    },
    aesKey,
    ciphertext,
  );

  return new TextDecoder().decode(decrypted);
}

// Helper: convert raw AES key bytes to CryptoKey
export async function importAESKeyFromBuffer(buffer) {
  return await crypto.subtle.importKey(
    "raw",
    buffer,
    { name: "AES-GCM" },
    false,
    ["decrypt", "encrypt"],
  );
}

export async function encryptAESGCM(message, aesKey) {
  const iv = crypto.getRandomValues(new Uint8Array(12)); // 96-bit IV
  const encoder = new TextEncoder();
  const data = encoder.encode(message);

  const encrypted = await crypto.subtle.encrypt(
    {
      name: "AES-GCM",
      iv: iv,
    },
    aesKey,
    data,
  );

  const encryptedBytes = new Uint8Array(encrypted);
  const result = new Uint8Array(iv.length + encryptedBytes.length);

  // Prepend IV
  result.set(iv, 0);
  result.set(encryptedBytes, iv.length);

  return btoa(String.fromCharCode(...result)); // Base64
}
