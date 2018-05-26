package esmsclient;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;
import lk.mobitel.esms.session.*;
import lk.mobitel.esms.message.*;
import lk.mobitel.esms.ws.*;
/**
 *
 * @author akalankad
 */
public class Main {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        // TODO code application logic here
        Main m=new Main();
        m.test();
    }

    public void test() {
		//create user object
		User user = new User();
		user.setUsername("USERNAME");
		user.setPassword("PASSWORD");
		
		//initiate session
		SessionManager sm = SessionManager.getInstance();
		System.out.println("Logging in.....");
		if ( !sm.isSession() ) {
			sm.login(user);
		}
		System.out.println("Logged in!");
		
		//create alias obj
		Alias alias = new Alias();
		alias.setAlias("ALIAS");
		
		//create SmsMessage object 
		SmsMessage msg = new SmsMessage();
		msg.setMessage("MESSAGE_BODY");
		System.out.println("Message length: " + msg.getMessage().length());
		msg.setSender(alias);
		msg.getRecipients().add("DST_NUMBER");
		
		//send sms
		try {
			SMSManager smsMgr = new SMSManager();
			System.out.println("Sending......");
			int ret = smsMgr.sendMessage(msg);
			System.out.println("Sent! " + ret);
		}catch(Exception ex){
			ex.printStackTrace();
		}

		/* get received messages for long number     
		SMSManager smsMgr = new SMSManager();
		LongNumber longNUmber=new LongNumber();
		longNUmber.setLongNumber("");
		try {
			List <SmsMessage> receivedMessages=smsMgr.getMessagesFromLongNumber(longNUmber);
			for(int i=0;i<receivedMessages.size();i++){
				System.out.println(receivedMessages.get(i).getSender().getAlias()+","+receivedMessages.get(i).getMessage());
			}
		} catch (NullSessionException ex) {
			Logger.getLogger(Main.class.getName()).log(Level.SEVERE, null, ex);
		}
		*/
    }

}
