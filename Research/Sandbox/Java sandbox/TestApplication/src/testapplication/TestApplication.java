package testapplication;

import java.math.BigInteger;
import java.util.Scanner;

public class TestApplication {
    public static void main(String args[]) {
        
        int numbersCount;
        BigInteger result;
        Scanner scanner = new Scanner(System.in);
        numbersCount = scanner.nextInt();
        result = scanner.nextBigInteger();
        
        BigInteger currentNumber;
        
        for (int i = 0; i < numbersCount - 1; i += 1) {
            currentNumber = scanner.nextBigInteger();
            result = result.xor(currentNumber);
        }
        
        System.out.println(result);
    }
}
