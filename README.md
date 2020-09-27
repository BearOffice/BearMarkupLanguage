# ConfReaderLib
 A class library to read or modify the config file reliably.  
  
# Memo
 Create a new conf file.  
 `ConfReader.Create(new[] { "Key A,Value A", "Key B,Value B,Comment B" }, "myapp.conf");`  
  
 Read a config file.  
 `var reader = new ConfReader("myapp.conf");`  
  
 Get key's value.  
 `reader.GetValue("Key A");`  
>Value A  
  
 Try to get key's value. This method returns bool.  
 `reader.TryGetValue("Key A", out string value);`  
>Value A  
  
 Change a key's value.  
 ```reader.ChangeValue("Key A", "New Value A");  
 reader.GetValue("Key A");```  
>Value A  
  
 Add a key.  
 `reader.AddConf(new[] { "Key C,Value C", "Key D,Value D" });`  