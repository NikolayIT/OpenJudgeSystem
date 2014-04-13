/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

package sandboxtests;

import java.lang.reflect.Constructor;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;

/**
 *
 * @author dininski
 */
public class SandboxTests {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws ClassNotFoundException, NoSuchMethodException, IllegalAccessException, IllegalArgumentException, InvocationTargetException, InstantiationException {
        
        // TODO: add checking for the user's package name.
        // get the instance of the user created class, containing main(String[] args)
        Class userClass = Class.forName("UserClass");
        Method userMethod = userClass.getMethod("main", String[].class);
        Constructor constructor = userClass.getConstructor();
        Object instance = constructor.newInstance();
        
        // set the security manager
        SandboxSecurityManager securityManager = new SandboxSecurityManager();
        System.setSecurityManager(securityManager);
        
        // TODO: Limit the user from using Reflection as well other potentially
        // dangerous libraries (java.lang ?)
        userMethod.invoke(userClass, (Object)args);
    }
}
