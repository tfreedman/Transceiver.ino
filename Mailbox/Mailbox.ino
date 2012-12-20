#include <VirtualWire.h>
#include <avr/sleep.h>

char inData[20];
char inChar;
byte index = 0;

void setup(void) {
  for (int i = 0; i < 25; i++) // Lower power usage
    pinMode(i, OUTPUT); 
  ADCSRA = 0; // Disable ADC on Teensy
  
  Serial.begin(9600);
  Serial.println("Init");
  vw_set_tx_pin(2);
  //vw_set_rx_pin(0);
  vw_set_ptt_inverted(false);
  vw_setup(1200);
  //vw_rx_start();
  pinMode(12, INPUT_PULLUP);
  pinMode(11, OUTPUT);
}

void loop(void) {
  if (Serial.available() > 0) {
    while(Serial.available() > 0) {
      inChar = Serial.read();
      if (inChar == '\r' || inChar == '\n')
        inChar == '\0';
      inData[index] = inChar;
      index++;
      inData[index] = '\0';
    }
    if (inData[0] != ':')
      Serial.println(inData);
    Serial.flush();
    index = 0;
  }
  else if (digitalRead(12) == 1) {
    strcpy(inData, "MBX:Open");
    Serial.println("MBX:Open");
  }

  uint8_t buf[VW_MAX_MESSAGE_LEN];
  uint8_t buflen = VW_MAX_MESSAGE_LEN;

  if (inData[0] != '\0') {
    vw_send((uint8_t *)inData, strlen(inData));
    vw_wait_tx();
    for (int i = 0; i < 20; i++)
      inData[i] = '\0';
  }

  /*if (vw_get_message(buf, &buflen)) {
    Serial.print("RECV: ");
    for (int i = 0; i < buflen; i++) {
      Serial.print((char)buf[i]);
    }
    Serial.println("");
    Serial.flush();
  }*/
}




