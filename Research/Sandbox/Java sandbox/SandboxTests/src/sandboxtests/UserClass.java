/*
* To change this license header, choose License Headers in Project Properties.
* To change this template file, choose Tools | Templates
* and open the template in the editor.
*/

import java.awt.Toolkit;
import java.awt.datatransfer.DataFlavor;
import java.awt.datatransfer.UnsupportedFlavorException;
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.Scanner;

/**
 *
 * @author dininski
 */
public class UserClass {
    public static void main(String[] args) throws IOException {
        // read the input
        try {
            PrintInput(args);
        } catch (Exception ex) {
            System.out.println(ex);
        }
        
        // access file
        try {
            OpenFile("textfile.txt");
        } catch (Exception ex) {
            System.out.println(String.format("Exception when trying to read a file. Exception: %s", ex));
        }
        
        // read clipboard - commented out because it throws an exception
        // as new threads are not allowed
        try {
            // ReadClipboard();
        } catch (Exception ex) {
            System.out.println(String.format("Exception when trying to read clipboard. Exception: %s", ex));
        }
        
        // try to start a new thread
        try {
            StartNewThread();
        } catch(Exception ex) {
            System.out.println(String.format("Exception when trying to start a new thread. Exception: %s", ex));
        }
        
        // try to use reflection
        try {
            UseReflection();
        } catch(Exception ex) {
            System.out.println(String.format("Exception when trying to user Reflection. Exception: %s", ex));
        }
        
        // read console
        ReadConsole();
    }
    
    private static void PrintInput(String[] args) {
        for (String arg : args) {
            System.out.println(arg);
        }
    }
    
    private static void OpenFile(String fileName) throws IOException {
        BufferedReader br = new BufferedReader(new FileReader(fileName));
        try {
            StringBuilder sb = new StringBuilder();
            String line = br.readLine();
            
            while (line != null) {
                sb.append(line);
                sb.append('\n');
                line = br.readLine();
            }
            
            String everything = sb.toString();
            System.out.println(everything);
        } finally {
            br.close();
        }
    }
    
    private static void ReadClipboard() throws UnsupportedFlavorException, IOException {
        String data = (String) Toolkit.getDefaultToolkit()
                .getSystemClipboard().getData(DataFlavor.stringFlavor);
    }
    
    private static void ReadConsole() {
        Scanner scanner = new Scanner(System.in);
        String enteredText = scanner.nextLine();
        System.out.println(enteredText);
    }
    
    private static void StartNewThread() {
        TestThread obj;
        obj = new TestThread();
        Thread thread;
        thread = new Thread(obj);
        thread.start();
    }
    
    static class TestThread extends Thread {
        @Override
        public synchronized void run() {
            for (int i = 0; i < 10; i++) {
                System.out.println("No");
            }
        }
    }
    
    private static void UseReflection() throws ClassNotFoundException {
        Class cl = Class.forName("UserClass");
        System.out.println(cl);
    }
}
