/*
* To change this license header, choose License Headers in Project Properties.
* To change this template file, choose Tools | Templates
* and open the template in the editor.
*/

package sandboxtests;

import java.io.FileDescriptor;
import java.lang.reflect.ReflectPermission;

/**
 *
 * @author dininski
 */
public class SandboxSecurityManager extends SecurityManager {
    public SandboxSecurityManager() {
    }
    
    @Override
    public void checkAccess(Thread thread) {
        throw new UnsupportedOperationException();
    }
}
