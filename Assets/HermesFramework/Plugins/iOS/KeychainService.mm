// Keychain Services を利用するために Security フレームワークを利用する
#import <Security/Security.h>

extern "C"
{
    // 指定したキーで値を保存する関数
    // - param
    //   - dataType:  値を保存する際に用いるキー
    //   - value: 保存したい値
    // - return
    //   - 保存時のステータスコードを返却する (0 以外は失敗)
    int addItem(const char *dataType, const char *value)
    {
        NSMutableDictionary* attributes = nil;
        NSMutableDictionary* query = [NSMutableDictionary dictionary];
        NSData* sata = [[NSString stringWithCString:value encoding:NSUTF8StringEncoding] dataUsingEncoding:NSUTF8StringEncoding];
    
        [query setObject:(id)kSecClassGenericPassword forKey:(id)kSecClass];
        [query setObject:(id)[NSString stringWithCString:dataType encoding:NSUTF8StringEncoding] forKey:(id)kSecAttrAccount];
    
        OSStatus err = SecItemCopyMatching((CFDictionaryRef)query, NULL);
    
        if (err == noErr) {
            attributes = [NSMutableDictionary dictionary];
            [attributes setObject:sata forKey:(id)kSecValueData];
            [attributes setObject:[NSDate date] forKey:(id)kSecAttrModificationDate];
    
            err = SecItemUpdate((CFDictionaryRef)query, (CFDictionaryRef)attributes);
            return (int)err;
        } else if (err == errSecItemNotFound) {
            attributes = [NSMutableDictionary dictionary];
            [attributes setObject:(id)kSecClassGenericPassword forKey:(id)kSecClass];
            [attributes setObject:(id)[NSString stringWithCString:dataType encoding:NSUTF8StringEncoding] forKey:(id)kSecAttrAccount];
            [attributes setObject:sata forKey:(id)kSecValueData];
            [attributes setObject:[NSDate date] forKey:(id)kSecAttrCreationDate];
            [attributes setObject:[NSDate date] forKey:(id)kSecAttrModificationDate];
            err = SecItemAdd((CFDictionaryRef)attributes, NULL);
            return (int)err;
        } else {
            return (int)err;
        }
    }
    
    // 指定したキーで値を取得する関数
    // - param
    //   - dataType: 値を取得する際に用いるキー
    // - return
    //   - キーに紐づく値、存在しなければ空文字が返却される
    char* getItem(const char *dataType)
    {
        NSMutableDictionary* query = [NSMutableDictionary dictionary];
        [query setObject:(id)kSecClassGenericPassword forKey:(id)kSecClass];
        [query setObject:(id)[NSString stringWithCString:dataType encoding:NSUTF8StringEncoding] forKey:(id)kSecAttrAccount];
        [query setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    
        CFDataRef cfresult = NULL;
        OSStatus err = SecItemCopyMatching((CFDictionaryRef)query, (CFTypeRef*)&cfresult);
    
        if (err == noErr) {
            NSData* passwordData = (__bridge_transfer NSData *)cfresult;
            const char* value = [[[NSString alloc] initWithData:passwordData encoding:NSUTF8StringEncoding] UTF8String];
            char *str = strdup(value);
            return str;
        } else {
            return NULL;
        }
    }
    
    // 指定したキーで値を削除する関数
    // - param
    //   - dataType:  値を削除する際に用いるキー
    // - return
    //   - 保存時のステータスコードを返却する (0 以外は失敗)
    int deleteItem(const char *dataType)
    {
        NSMutableDictionary* query = [NSMutableDictionary dictionary];
        [query setObject:(id)kSecClassGenericPassword forKey:(id)kSecClass];
        [query setObject:(id)[NSString stringWithCString:dataType encoding:NSUTF8StringEncoding] forKey:(id)kSecAttrAccount];
    
        OSStatus err = SecItemDelete((CFDictionaryRef)query);
    
        if (err == noErr) {
            return 0;
        } else {
            return (int)err;
        }
    }
}